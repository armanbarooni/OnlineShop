using FluentValidation;
using OnlineShop.Application.Features.Product.Command.Create;
using OnlineShop.Application.Validators.Product;

namespace OnlineShop.Application.Features.Product.Command.Create
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Product)
                .NotNull()
                .SetValidator(new CreateProductDtoValidator());
        }
    }
}

