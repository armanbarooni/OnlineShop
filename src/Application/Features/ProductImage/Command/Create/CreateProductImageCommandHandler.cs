using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductImage;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Features.ProductImage.Command.Create
{
    public class CreateProductImageCommandHandler : IRequestHandler<CreateProductImageCommand, Result<ProductImageDto>>
    {
        private readonly IProductImageRepository _repository;
        private readonly IMapper _mapper;

        public CreateProductImageCommandHandler(IProductImageRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductImageDto>> Handle(CreateProductImageCommand request, CancellationToken cancellationToken)
        {
            var productImage = ProductImage.Create(
                request.ProductImage.ProductId,
                request.ProductImage.ImageUrl,
                request.ProductImage.AltText,
                request.ProductImage.DisplayOrder,
                request.ProductImage.IsPrimary
            );

            await _repository.AddAsync(productImage, cancellationToken);
            return Result<ProductImageDto>.Success(_mapper.Map<ProductImageDto>(productImage));
        }
    }
}
