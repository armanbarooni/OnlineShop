using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductInventory;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductInventory.Command.Create
{
    public class CreateProductInventoryCommandHandler : IRequestHandler<CreateProductInventoryCommand, Result<ProductInventoryDto>>
    {
        private readonly IProductInventoryRepository _repository;
        private readonly IMapper _mapper;

        public CreateProductInventoryCommandHandler(IProductInventoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductInventoryDto>> Handle(CreateProductInventoryCommand request, CancellationToken cancellationToken)
        {
            if (request.ProductInventory == null)
                return Result<ProductInventoryDto>.Failure("ProductInventory data is required");

            var productInventory = Domain.Entities.ProductInventory.Create(
                request.ProductInventory.ProductId,
                request.ProductInventory.AvailableQuantity,
                request.ProductInventory.ReservedQuantity,
                request.ProductInventory.SoldQuantity,
                request.ProductInventory.CostPrice,
                request.ProductInventory.SellingPrice
            );

            await _repository.AddAsync(productInventory, cancellationToken);
            return Result<ProductInventoryDto>.Success(_mapper.Map<ProductInventoryDto>(productInventory));
        }
    }
}

