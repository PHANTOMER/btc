using System.Collections.Generic;
using System.Threading.Tasks;
using Btc.Contracts.Dto;
using Btc.Contracts.Entities;

namespace Btc.Bitcoin
{
    public interface IBitcoinApiWrapper
    {
        Task<string> TrySendBtcAsync(SendBtcDto btc, string wallet);

        Task<IEnumerable<Wallet>> TryListWalletsWithBalancesAsync();

        Task<IEnumerable<Transaction>> TryListTransactionsAsync(string walletName);
    }
}