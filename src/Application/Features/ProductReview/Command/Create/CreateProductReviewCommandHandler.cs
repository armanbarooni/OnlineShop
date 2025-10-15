using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductReview;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductReview.Command.Create
{
    public class CreateProductReviewCommandHandler : IRequestHandler<CreateProductReviewCommand, Result<ProductReviewDto>>
    {
        private readonly IProductReviewRepository _repository;
        private readonly IMapper _mapper;

        public CreateProductReviewCommandHandler(IProductReviewRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductReviewDto>> Handle(CreateProductReviewCommand request, CancellationToken cancellationToken)
        {
            if (request.ProductReview == null)
                return Result<ProductReviewDto>.Failure("ProductReview data is required");

            // Check if user has already reviewed this product
            var hasReviewed = await _repository.HasUserReviewedProductAsync(request.UserId, request.ProductReview.ProductId, cancellationToken);
            if (hasReviewed)
                return Result<ProductReviewDto>.Failure("User has already reviewed this product");

            var productReview = Domain.Entities.ProductReview.Create(
                request.ProductReview.ProductId,
                request.UserId,
                request.ProductReview.Title,
                request.ProductReview.Comment,
                request.ProductReview.Rating
            );

            await _repository.AddAsync(productReview, cancellationToken);
            return Result<ProductReviewDto>.Success(_mapper.Map<ProductReviewDto>(productReview));
        }
    }
}

