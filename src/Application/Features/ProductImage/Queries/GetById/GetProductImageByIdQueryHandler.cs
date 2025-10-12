using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductImage;

namespace OnlineShop.Application.Features.ProductImage.Queries.GetById
{
    public class GetProductImageByIdQueryHandler : IRequestHandler<GetProductImageByIdQuery, Result<ProductImageDto>>
    {
        private readonly IProductImageRepository _repository;
        private readonly IMapper _mapper;

        public GetProductImageByIdQueryHandler(IProductImageRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductImageDto>> Handle(GetProductImageByIdQuery request, CancellationToken cancellationToken)
        {
            var productImage = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (productImage == null)
                return Result<ProductImageDto>.Failure("ProductImage not found");

            return Result<ProductImageDto>.Success(_mapper.Map<ProductImageDto>(productImage));
        }
    }
}
