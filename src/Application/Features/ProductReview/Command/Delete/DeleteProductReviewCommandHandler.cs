using MediatR;
using OnlineShop.Application.Common.Models;


using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.ProductReview.Command.Delete
{
    public class DeleteProductReviewCommandHandler(IProductReviewRepository repository)
        : IRequestHandler<DeleteProductReviewCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteProductReviewCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var productReview = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (productReview == null)
                    return Result<bool>.Failure("نظر محصول یافت نشد");

                productReview.Delete(null);
                await repository.UpdateAsync(productReview, cancellationToken);
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطا در حذف نظر محصول: {ex.Message}");
            }
        }
    }
}



