using FluentValidation;
using OnlineShop.Application.DTOs.ProductVariant;

namespace OnlineShop.Application.Validators
{
    public class UpdateProductVariantDtoValidator : AbstractValidator<UpdateProductVariantDto>
    {
        public UpdateProductVariantDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("شناسه تنوع محصول الزامی است");

            RuleFor(x => x.Size)
                .NotEmpty().WithMessage("سایز الزامی است")
                .MaximumLength(20).WithMessage("سایز نمی‌تواند بیش از 20 کاراکتر باشد");

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("رنگ الزامی است")
                .MaximumLength(50).WithMessage("رنگ نمی‌تواند بیش از 50 کاراکتر باشد");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU الزامی است")
                .MaximumLength(50).WithMessage("SKU نمی‌تواند بیش از 50 کاراکتر باشد");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("موجودی نمی‌تواند منفی باشد");

            RuleFor(x => x.AdditionalPrice)
                .GreaterThanOrEqualTo(0).WithMessage("قیمت اضافی نمی‌تواند منفی باشد")
                .When(x => x.AdditionalPrice.HasValue);

            RuleFor(x => x.Barcode)
                .MaximumLength(50).WithMessage("بارکد نمی‌تواند بیش از 50 کاراکتر باشد")
                .When(x => !string.IsNullOrEmpty(x.Barcode));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("ترتیب نمایش نمی‌تواند منفی باشد");
        }
    }
}
