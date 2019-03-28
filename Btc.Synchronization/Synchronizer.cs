using System.Linq;
using System.Threading.Tasks;
using Btc.Bitcoin;
using Btc.DataAccess.Repositories;

namespace Btc.Synchronization
{
    public class Synchronizer : ISynchronizer
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IBitcoinApiWrapper _bitcoinApiWrapper;

        public Synchronizer(IWalletRepository walletRepository, IBitcoinApiWrapper bitcoinApiWrapper)
        {
            _walletRepository = walletRepository;
            _bitcoinApiWrapper = bitcoinApiWrapper;
        }

        public async Task SynchronizeWalletsAsync()
        {
            var walletsWithBalances = (await _bitcoinApiWrapper.TryListWalletsWithBalancesAsync().ConfigureAwait(false)).ToList();

            await _walletRepository.TryUpdateWalletsAsync(walletsWithBalances).ConfigureAwait(false);

            var walletsWithTransactions = (await Task.WhenAll(walletsWithBalances.Select(async wallet =>
            {
                var transactions = await _bitcoinApiWrapper.TryListTransactionsAsync(wallet.Name).ConfigureAwait(false);

                foreach (var transaction in transactions)
                {
                    wallet.Transactions.Add(transaction);
                }

                return wallet;
            })).ConfigureAwait(false)).ToList();

            await _walletRepository.TryUpdateTransactionsAsync(walletsWithTransactions).ConfigureAwait(false);
        }
    }
}
