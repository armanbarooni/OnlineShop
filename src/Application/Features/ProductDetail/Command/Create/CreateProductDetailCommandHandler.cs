using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductDetail;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductDetail.Command.Create
{
    public class CreateProductDetailCommandHandler : IRequestHandler<CreateProductDetailCommand, Result<ProductDetailDto>>
    {
        private readonly IProductDetailRepository _repository;
        private readonly IMapper _mapper;

        public CreateProductDetailCommandHandler(IProductDetailRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductDetailDto>> Handle(CreateProductDetailCommand request, CancellationToken cancellationToken)
        {
            if (request.ProductDetail == null)
                return Result<ProductDetailDto>.Failure("ProductDetail data is required");

            var productDetail = Domain.Entities.ProductDetail.Create(
                request.ProductDetail.ProductId,
                request.ProductDetail.Key,
                request.ProductDetail.Value,
                request.ProductDetail.Description,
                request.ProductDetail.DisplayOrder
            );

            await _repository.AddAsync(productDetail, cancellationToken);
            return Result<ProductDetailDto>.Success(_mapper.Map<ProductDetailDto>(productDetail));
        }
    }
}

