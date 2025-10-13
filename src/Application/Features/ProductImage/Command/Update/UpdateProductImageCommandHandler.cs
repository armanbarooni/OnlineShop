using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductImage;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.ProductImage.Command.Update
{
    public class UpdateProductImageCommandHandler : IRequestHandler<UpdateProductImageCommand, Result<ProductImageDto>>
    {
        private readonly IProductImageRepository _repository;
        private readonly IMapper _mapper;

        public UpdateProductImageCommandHandler(IProductImageRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductImageDto>> Handle(UpdateProductImageCommand request, CancellationToken cancellationToken)
        {
            if (request.ProductImage == null)
                return Result<ProductImageDto>.Failure("ProductImage data is required");

            var productImage = await _repository.GetByIdAsync(request.ProductImage.Id, cancellationToken);
            if (productImage == null)
                return Result<ProductImageDto>.Failure("ProductImage not found");

            productImage.Update(
                request.ProductImage.ImageUrl,
                request.ProductImage.AltText,
                request.ProductImage.Title,
                request.ProductImage.DisplayOrder,
                request.ProductImage.IsPrimary,
                productImage.ImageType, // Keep existing image type
                request.ProductImage.FileSize,
                request.ProductImage.MimeType,
                null
            );

            await _repository.UpdateAsync(productImage, cancellationToken);
            return Result<ProductImageDto>.Success(_mapper.Map<ProductImageDto>(productImage));
        }
    }
}
