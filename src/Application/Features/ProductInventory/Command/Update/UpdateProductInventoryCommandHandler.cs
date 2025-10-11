using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductInventory;
using OnlineShop.Infrastructure.Persistence.Repositories;

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

            productInventory.UpdateInventory(
                request.ProductInventory.AvailableQuantity,
                request.ProductInventory.ReservedQuantity,
                request.ProductInventory.SoldQuantity,
                request.ProductInventory.CostPrice,
                request.ProductInventory.SellingPrice,
                null
            );

            await _repository.UpdateAsync(productInventory, cancellationToken);
            return Result<ProductInventoryDto>.Success(_mapper.Map<ProductInventoryDto>(productInventory));
        }
    }
}
