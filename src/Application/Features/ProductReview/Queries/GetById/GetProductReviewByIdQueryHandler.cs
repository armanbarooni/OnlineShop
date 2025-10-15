using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductReview;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.ProductReview.Queries.GetById
{
    public class GetProductReviewByIdQueryHandler(
        IProductReviewRepository repository,
        IMapper mapper) : IRequestHandler<GetProductReviewByIdQuery, Result<ProductReviewDto>>
    {
        public async Task<Result<ProductReviewDto>> Handle(GetProductReviewByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var productReview = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (productReview == null)
                    return Result<ProductReviewDto>.Failure("نظر محصول یافت نشد");

                var dto = mapper.Map<ProductReviewDto>(productReview);
                return Result<ProductReviewDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<ProductReviewDto>.Failure($"خطا در دریافت نظر محصول: {ex.Message}");
            }
        }
    }
}



