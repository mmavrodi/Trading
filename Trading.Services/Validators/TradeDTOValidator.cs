using FluentValidation;
using Trading.DTO.Models;

namespace Trading.Services.Validators
{
    public class TradeDTOValidator : AbstractValidator<TradeDTO>
    {
        public TradeDTOValidator()
        {
            RuleFor(x => x.ClientOrderId).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Symbol).NotEmpty().MaximumLength(10);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.Type).IsInEnum();
            RuleFor(x => x.Side).IsInEnum();
        }
    }
}
