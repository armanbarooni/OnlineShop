using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductReview;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductReview.Command.Reject
{
    public class RejectProductReviewCommandHandler : IRequestHandler<RejectProductReviewCommand, Result<ProductReviewDto>>
    {
        private readonly IProductReviewRepository _repository;
        private readonly IMapper _mapper;

        public RejectProductReviewCommandHandler(IProductReviewRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductReviewDto>> Handle(RejectProductReviewCommand request, CancellationToken cancellationToken)
        {
            var productReview = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (productReview == null)
                return Result<ProductReviewDto>.Failure("ProductReview not found");

            productReview.Reject("Admin");

            await _repository.UpdateAsync(productReview, cancellationToken);
            return Result<ProductReviewDto>.Success(_mapper.Map<ProductReviewDto>(productReview));
        }
    }
}

