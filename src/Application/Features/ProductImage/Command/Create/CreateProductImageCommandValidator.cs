using FluentValidation;
using OnlineShop.Application.Features.ProductImage.Command.Create;
using OnlineShop.Application.Validators.ProductImage;

namespace OnlineShop.Application.Features.ProductImage.Command.Create
{
    public class CreateProductImageCommandValidator : AbstractValidator<CreateProductImageCommand>
    {
        public CreateProductImageCommandValidator()
        {
            RuleFor(x => x.ProductImage)
                .NotNull()
                .SetValidator(new CreateProductImageDtoValidator());
        }
    }
}

