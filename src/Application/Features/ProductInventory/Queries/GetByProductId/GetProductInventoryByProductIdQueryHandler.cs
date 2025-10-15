using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductInventory;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.ProductInventory.Queries.GetByProductId
{
    public class GetProductInventoryByProductIdQueryHandler : IRequestHandler<GetProductInventoryByProductIdQuery, Result<ProductInventoryDto>>
    {
        private readonly IProductInventoryRepository _repository;
        private readonly IMapper _mapper;

        public GetProductInventoryByProductIdQueryHandler(IProductInventoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductInventoryDto>> Handle(GetProductInventoryByProductIdQuery request, CancellationToken cancellationToken)
        {
            var productInventory = await _repository.GetByProductIdAsync(request.ProductId, cancellationToken);
            if (productInventory == null)
                return Result<ProductInventoryDto>.Failure("ProductInventory not found");

            return Result<ProductInventoryDto>.Success(_mapper.Map<ProductInventoryDto>(productInventory));
        }
    }
}


