using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Command.Update
{
    public class UpdateCartCommandHandler(
        ICartRepository repository,
        IMapper mapper) : IRequestHandler<UpdateCartCommand, Result<CartDto>>
    {
        public async Task<Result<CartDto>> Handle(UpdateCartCommand request, CancellationToken cancellationToken)
        {
            if (request.Cart == null)
                return Result<CartDto>.Failure("داده‌های سبد خرید نباید خالی باشد");

            try
            {
                var cart = await repository.GetByIdAsync(request.Cart.Id, cancellationToken);
                
                if (cart == null)
                    return Result<CartDto>.Failure("سبد خرید یافت نشد");

                cart.Update(
                    request.Cart.CartName,
                    request.Cart.IsActive,
                    request.Cart.ExpiresAt,
                    request.Cart.Notes,
                    null
                );

                await repository.UpdateAsync(cart, cancellationToken);
                return Result<CartDto>.Success(mapper.Map<CartDto>(cart));
            }
            catch (Exception ex)
            {
                return Result<CartDto>.Failure($"خطا در به‌روزرسانی سبد خرید: {ex.Message}");
            }
        }
    }
}

