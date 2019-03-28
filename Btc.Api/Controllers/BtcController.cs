using System.Collections.Generic;
using System.Threading.Tasks;
using Btc.Api.Extensions;
using Btc.Api.Infrastructure;
using Btc.Business;
using Btc.Contracts.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Btc.Api.Controllers
{
    [Route("api/[controller]")]
    public class BtcController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public BtcController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        [Route("get-last")]
        public async Task<ServiceResponse<IEnumerable<TransactionDto>>> GetLast()
        {
            var transactions = await _transactionService.GetLastTransactionsAsync().ConfigureAwait(false);
            return ServiceResponse<IEnumerable<TransactionDto>>.New(true, transactions);
        }

        [HttpPost]
        [Route("send-btc")]
        public async Task<ServiceResponse<SendBtcResultDto>> SendBtc([FromBody] SendBtcDto btc)
        {
            ValidateModelState();

            SendBtcResultDto result = await _transactionService.SendBtcAsync(btc).ConfigureAwait(false);
            return ServiceResponse<SendBtcResultDto>.New(true, result);
        }
        
        protected virtual void ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                throw this.ModelState.FromModelState();
            }
        }
    }
}
