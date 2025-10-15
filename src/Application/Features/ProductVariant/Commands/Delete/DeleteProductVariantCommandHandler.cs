using MediatR;
using OnlineShop.Application.Common.Models;


using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.ProductVariant.Commands.Delete
{
    public class DeleteProductVariantCommandHandler : IRequestHandler<DeleteProductVariantCommand, Result<bool>>
    {
        private readonly IProductVariantRepository _repository;

        public DeleteProductVariantCommandHandler(IProductVariantRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var productVariant = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (productVariant == null)
                {
                    return Result<bool>.Failure("تنوع محصول مورد نظر یافت نشد");
                }

                await _repository.DeleteAsync(request.Id, cancellationToken);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطا در حذف تنوع محصول: {ex.Message}");
            }
        }
    }
}


