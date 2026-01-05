using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.User.Commands.DeleteAddress
{
    public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, Result<string>>
    {
        private readonly IUserAddressRepository _addressRepository;

        public DeleteAddressCommandHandler(IUserAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<Result<string>> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetByIdAsync(request.AddressId, cancellationToken);
            if (address == null)
            {
                 return Result<string>.Failure("آدرس یافت نشد");
            }

            if (address.UserId != request.UserId)
            {
                 return Result<string>.Failure("دسترسی غیرمجاز");
            }

            await _addressRepository.DeleteAsync(address.Id, cancellationToken);
            return Result<string>.Success("آدرس با موفقیت حذف شد");
        }
    }
}
