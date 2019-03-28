using System.Collections.Generic;
using System.Threading.Tasks;
using Btc.Contracts.Dto;

namespace Btc.Business
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionDto>> GetLastTransactionsAsync();
        Task<SendBtcResultDto> SendBtcAsync(SendBtcDto btc);
    }
}