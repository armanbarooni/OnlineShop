using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductReview;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.ProductReview.Command.Approve
{
    public class ApproveProductReviewCommandHandler : IRequestHandler<ApproveProductReviewCommand, Result<ProductReviewDto>>
    {
        private readonly IProductReviewRepository _repository;
        private readonly IMapper _mapper;

        public ApproveProductReviewCommandHandler(IProductReviewRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductReviewDto>> Handle(ApproveProductReviewCommand request, CancellationToken cancellationToken)
        {
            var productReview = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (productReview == null)
                return Result<ProductReviewDto>.Failure("ProductReview not found");

            productReview.Approve("Admin", request.AdminNotes);

            await _repository.UpdateAsync(productReview, cancellationToken);
            return Result<ProductReviewDto>.Success(_mapper.Map<ProductReviewDto>(productReview));
        }
    }
}
