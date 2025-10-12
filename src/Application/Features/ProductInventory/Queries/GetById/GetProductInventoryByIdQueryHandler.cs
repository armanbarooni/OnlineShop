using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductInventory;

namespace OnlineShop.Application.Features.ProductInventory.Queries.GetById
{
    public class GetProductInventoryByIdQueryHandler : IRequestHandler<GetProductInventoryByIdQuery, Result<ProductInventoryDto>>
    {
        private readonly IProductInventoryRepository _repository;
        private readonly IMapper _mapper;

        public GetProductInventoryByIdQueryHandler(IProductInventoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductInventoryDto>> Handle(GetProductInventoryByIdQuery request, CancellationToken cancellationToken)
        {
            var productInventory = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (productInventory == null)
                return Result<ProductInventoryDto>.Failure("ProductInventory not found");

            return Result<ProductInventoryDto>.Success(_mapper.Map<ProductInventoryDto>(productInventory));
        }
    }
}
