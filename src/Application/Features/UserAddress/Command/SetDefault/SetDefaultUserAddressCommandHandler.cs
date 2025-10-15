using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserAddress.Command.SetDefault
{
    public class SetDefaultUserAddressCommandHandler : IRequestHandler<SetDefaultUserAddressCommand, Result<bool>>
    {
        private readonly IUserAddressRepository _repository;

        public SetDefaultUserAddressCommandHandler(IUserAddressRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(SetDefaultUserAddressCommand request, CancellationToken cancellationToken)
        {
            await _repository.SetAsDefaultAsync(request.UserId, request.AddressId, cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}

