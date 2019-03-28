using System.Collections.Generic;
using System.Threading.Tasks;
using Btc.Contracts.Entities;

namespace Btc.DataAccess.Repositories
{
    public interface IWalletRepository
    {
        Task<IEnumerable<Transaction>> GetLastTransactionsAsync();

        Task TryUpdateTransactionsAsync(IEnumerable<Wallet> walletsWithTransactions);

        Task TryUpdateWalletsAsync(IEnumerable<Wallet> wallets);
    }
}