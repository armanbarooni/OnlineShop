using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;

namespace OnlineShop.Application.Features.Cart.Command.Delete
{
    public class DeleteCartCommandHandler(ICartRepository repository)
        : IRequestHandler<DeleteCartCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteCartCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var cart = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (cart == null)
                    return Result<bool>.Failure("سبد خرید یافت نشد");

                cart.Delete(null);
                await repository.UpdateAsync(cart, cancellationToken);
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطا در حذف سبد خرید: {ex.Message}");
            }
        }
    }
}

