using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductInventory;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.ProductInventory.Queries.GetAll
{
    public class GetAllProductInventoriesQueryHandler : IRequestHandler<GetAllProductInventoriesQuery, Result<IEnumerable<ProductInventoryDto>>>
    {
        private readonly IProductInventoryRepository _repository;
        private readonly IMapper _mapper;

        public GetAllProductInventoriesQueryHandler(IProductInventoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ProductInventoryDto>>> Handle(GetAllProductInventoriesQuery request, CancellationToken cancellationToken)
        {
            var productInventories = await _repository.GetAllAsync(cancellationToken);
            var productInventoryDtos = _mapper.Map<IEnumerable<ProductInventoryDto>>(productInventories);
            return Result<IEnumerable<ProductInventoryDto>>.Success(productInventoryDtos);
        }
    }
}


