using FluentValidation;
using OnlineShop.Application.DTOs.Auth;

namespace OnlineShop.Application.Validators.Auth
{
    public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token الزامی است");
        }
    }
}
