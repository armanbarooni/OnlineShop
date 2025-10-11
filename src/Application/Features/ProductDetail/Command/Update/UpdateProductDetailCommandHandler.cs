using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductDetail;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.ProductDetail.Command.Update
{
    public class UpdateProductDetailCommandHandler : IRequestHandler<UpdateProductDetailCommand, Result<ProductDetailDto>>
    {
        private readonly IProductDetailRepository _repository;
        private readonly IMapper _mapper;

        public UpdateProductDetailCommandHandler(IProductDetailRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductDetailDto>> Handle(UpdateProductDetailCommand request, CancellationToken cancellationToken)
        {
            if (request.ProductDetail == null)
                return Result<ProductDetailDto>.Failure("ProductDetail data is required");

            var productDetail = await _repository.GetByIdAsync(request.ProductDetail.Id, cancellationToken);
            if (productDetail == null)
                return Result<ProductDetailDto>.Failure("ProductDetail not found");

            productDetail.Update(
                request.ProductDetail.Key,
                request.ProductDetail.Value,
                request.ProductDetail.Description,
                request.ProductDetail.DisplayOrder,
                null
            );

            await _repository.UpdateAsync(productDetail, cancellationToken);
            return Result<ProductDetailDto>.Success(_mapper.Map<ProductDetailDto>(productDetail));
        }
    }
}
