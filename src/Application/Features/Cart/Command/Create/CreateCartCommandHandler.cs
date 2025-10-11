using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.Cart.Command.Create
{
    public class CreateCartCommandHandler : IRequestHandler<CreateCartCommand, Result<CartDto>>
    {
        private readonly ICartRepository _repository;
        private readonly IMapper _mapper;

        public CreateCartCommandHandler(ICartRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<CartDto>> Handle(CreateCartCommand request, CancellationToken cancellationToken)
        {
            if (request.Cart == null)
                return Result<CartDto>.Failure("Cart data is required");

            var cart = Domain.Entities.Cart.Create(
                request.UserId,
                request.Cart.SessionId,
                request.Cart.CartName,
                true,
                request.Cart.ExpiresAt
            );

            await _repository.AddAsync(cart, cancellationToken);
            return Result<CartDto>.Success(_mapper.Map<CartDto>(cart));
        }
    }
}
