using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductReview;

namespace OnlineShop.Application.Features.ProductReview.Queries.GetAll
{
    public class GetAllProductReviewsQueryHandler(
        IProductReviewRepository repository,
        IMapper mapper) : IRequestHandler<GetAllProductReviewsQuery, Result<List<ProductReviewDto>>>
    {
        public async Task<Result<List<ProductReviewDto>>> Handle(GetAllProductReviewsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var productReviews = await repository.GetAllAsync(cancellationToken);
                var dtoList = mapper.Map<List<ProductReviewDto>>(productReviews);
                return Result<List<ProductReviewDto>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                return Result<List<ProductReviewDto>>.Failure($"خطا در دریافت لیست نظرات محصولات: {ex.Message}");
            }
        }
    }
}

