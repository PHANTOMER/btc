using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Btc.Contracts;
using Btc.Contracts.Entities;
using Btc.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Btc.DataAccess.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private const int MaxConfirmationsForRecent = 3;
        private const int MaxConfirmations = 6;

        private readonly BtcContext _context;
        private readonly ILogger<WalletRepository> _logger;

        public WalletRepository(BtcContext context, ILogger<WalletRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task TryUpdateWalletsAsync(IEnumerable<Wallet> wallets)
        {
            try
            {
                var dbWallets = await _context.Wallets.ToListAsync().ConfigureAwait(false);

                foreach (var pair in wallets.GroupJoin(
                    dbWallets,
                    x => x.Name,
                    x => x.Name,
                    (x, y) => new
                    {
                        New = x,
                        Old = y.FirstOrDefault()
                    }))
                {
                    if (pair.Old != null)
                    {
                        pair.Old.Balance = pair.New.Balance;
                    }
                    else
                    {
                        pair.New.Id = Guid.NewGuid();
                        _context.Wallets.Add(pair.New);
                    }
                }

                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occured while updating wallets");
                throw;
            }
        }

        public async Task TryUpdateTransactionsAsync(IEnumerable<Wallet> walletsWithTransactions)
        {
            try
            {
                var newWallets = walletsWithTransactions.ToList();

                var walletNames = newWallets.Select(x => x.Name);

                var dbWallets = await _context.Wallets
                    .Include(x => x.Transactions)
                    .Where(x => walletNames.Contains(x.Name))
                    .ToListAsync()
                    .ConfigureAwait(false);

                foreach (var walletPair in newWallets.Join(
                    dbWallets,
                    x => x.Name,
                    x => x.Name,
                    (x, y) => new
                    {
                        NewWallet = x,
                        OldWallet = y
                    }))
                {
                    foreach (var transactionPair in walletPair.NewWallet.Transactions.GroupJoin(
                        walletPair.OldWallet.Transactions,
                        x => new
                        {
                            x.TxId,
                            x.TransactionType
                        },
                        x => new
                        {
                            x.TxId,
                            x.TransactionType
                        },
                        (x, y) => new
                        {
                            NewTransaction = x,
                            OldTransaction = y.FirstOrDefault()
                        }))
                    {
                        if (transactionPair.OldTransaction == null)
                        {
                            transactionPair.NewTransaction.Id = Guid.NewGuid();
                            transactionPair.NewTransaction.WalletId = walletPair.OldWallet.Id;
                            _context.Transactions.Add(transactionPair.NewTransaction);
                        }
                        else
                        {
                            if (transactionPair.OldTransaction.Confirmation <= MaxConfirmations)
                            {
                                transactionPair.OldTransaction.Confirmation = transactionPair.NewTransaction.Confirmation;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occured while updating transactions");
                throw;
            }
        }

        public async Task<IEnumerable<Transaction>> GetLastTransactionsAsync()
        {
            var param = new SqlParameter("@confirmation", System.Data.SqlDbType.Int)
            {
                Value = MaxConfirmationsForRecent
            };

            try
            {
                var transactions = await _context.Transactions
                    .FromSql("GetLastTransactions @confirmation", param)
                    .ToListAsync()
                    .ConfigureAwait(false);

                return transactions;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occured while getting last transactions");
                throw new Exception(Errors.DatabaseError);
            }
        }
    }
}
