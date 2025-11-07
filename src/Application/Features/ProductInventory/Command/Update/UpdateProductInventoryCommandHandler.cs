using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductInventory;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductInventory.Command.Update
{
    public class UpdateProductInventoryCommandHandler : IRequestHandler<UpdateProductInventoryCommand, Result<ProductInventoryDto>>
    {
        private readonly IProductInventoryRepository _repository;
        private readonly IMapper _mapper;

        public UpdateProductInventoryCommandHandler(IProductInventoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductInventoryDto>> Handle(UpdateProductInventoryCommand request, CancellationToken cancellationToken)
        {
            if (request.ProductInventory == null)
                return Result<ProductInventoryDto>.Failure("ProductInventory data is required");

            var productInventory = await _repository.GetByIdAsync(request.ProductInventory.Id, cancellationToken);
            if (productInventory == null)
                return Result<ProductInventoryDto>.Failure("ProductInventory not found");

            // Preserve existing values when fields are not provided (null)
            var newAvailable = request.ProductInventory.AvailableQuantity
                                ?? request.ProductInventory.Quantity
                                ?? productInventory.AvailableQuantity;
            var newReserved = request.ProductInventory.ReservedQuantity ?? productInventory.ReservedQuantity;
            var newSold = request.ProductInventory.SoldQuantity ?? productInventory.SoldQuantity;
            var newCost = request.ProductInventory.CostPrice ?? productInventory.CostPrice;
            var newSelling = request.ProductInventory.SellingPrice ?? productInventory.SellingPrice;

            productInventory.UpdateInventory(
                newAvailable,
                newReserved,
                newSold,
                newCost,
                newSelling,
                null
            );

            await _repository.UpdateAsync(productInventory, cancellationToken);
            return Result<ProductInventoryDto>.Success(_mapper.Map<ProductInventoryDto>(productInventory));
        }
    }
}

