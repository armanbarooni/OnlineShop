using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductReview;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductReview.Queries.GetByProductId
{
    public class GetProductReviewsByProductIdQueryHandler : IRequestHandler<GetProductReviewsByProductIdQuery, Result<IEnumerable<ProductReviewDto>>>
    {
        private readonly IProductReviewRepository _repository;
        private readonly IMapper _mapper;

        public GetProductReviewsByProductIdQueryHandler(IProductReviewRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ProductReviewDto>>> Handle(GetProductReviewsByProductIdQuery request, CancellationToken cancellationToken)
        {
            var productReviews = await _repository.GetApprovedReviewsAsync(request.ProductId, cancellationToken);
            var productReviewDtos = _mapper.Map<IEnumerable<ProductReviewDto>>(productReviews);
            return Result<IEnumerable<ProductReviewDto>>.Success(productReviewDtos);
        }
    }
}

