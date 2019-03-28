using Btc.Contracts.Dto;
using FluentValidation;

namespace Btc.Api.Validation
{
    public class SendBtcValidator : AbstractValidator<SendBtcDto>
    {
        public SendBtcValidator()
        {
            RuleFor(x => x.Address).NotNull().NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
        }
    }
}
