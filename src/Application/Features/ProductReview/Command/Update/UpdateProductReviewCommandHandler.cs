using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductReview;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductReview.Command.Update
{
    public class UpdateProductReviewCommandHandler : IRequestHandler<UpdateProductReviewCommand, Result<ProductReviewDto>>
    {
        private readonly IProductReviewRepository _repository;
        private readonly IMapper _mapper;

        public UpdateProductReviewCommandHandler(IProductReviewRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductReviewDto>> Handle(UpdateProductReviewCommand request, CancellationToken cancellationToken)
        {
            if (request.ProductReview == null)
                return Result<ProductReviewDto>.Failure("ProductReview data is required");

            var productReview = await _repository.GetByIdAsync(request.ProductReview.Id, cancellationToken);
            if (productReview == null)
                return Result<ProductReviewDto>.Failure("ProductReview not found");

            productReview.Update(
                request.ProductReview.Title,
                request.ProductReview.Comment,
                request.ProductReview.Rating,
                null
            );

            await _repository.UpdateAsync(productReview, cancellationToken);
            return Result<ProductReviewDto>.Success(_mapper.Map<ProductReviewDto>(productReview));
        }
    }
}

