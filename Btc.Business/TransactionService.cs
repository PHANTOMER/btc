using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Btc.Bitcoin;
using Btc.Contracts;
using Btc.Contracts.Dto;
using Btc.DataAccess.Repositories;

namespace Btc.Business
{
    public class TransactionService : ITransactionService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IBitcoinApiWrapper _bitcoinApiWrapper;

        public TransactionService(IWalletRepository walletRepository, IBitcoinApiWrapper bitcoinApiWrapper)
        {
            _walletRepository = walletRepository;
            _bitcoinApiWrapper = bitcoinApiWrapper;
        }

        public async Task<IEnumerable<TransactionDto>> GetLastTransactionsAsync()
        {
            var transactions = await _walletRepository.GetLastTransactionsAsync().ConfigureAwait(false);
            return transactions.Select(x => new TransactionDto()
            {
                Address = x.Address,
                Amount = x.Amount,
                Confirmation = x.Confirmation,
                Date = x.TxDate
            });
        }

        public async Task<SendBtcResultDto> SendBtcAsync(SendBtcDto btc)
        {
            var wallets = await _bitcoinApiWrapper.TryListWalletsWithBalancesAsync().ConfigureAwait(false);
            
            var walletsList = wallets.Where(x => x.Balance > btc.Amount).ToList();
            if (walletsList.Count == 0)
            {
                throw new Exception(Errors.NotEnoughFundsError);
            }

            var rand = new Random(DateTime.Now.Millisecond);
            var randIndex = rand.Next(0, walletsList.Count);
            var randomWallet = walletsList[randIndex];

            string transactionId = await _bitcoinApiWrapper.TrySendBtcAsync(btc, randomWallet.Name).ConfigureAwait(false);
            return new SendBtcResultDto()
            {
                TransactionId = transactionId
            };
        }
    }
}
