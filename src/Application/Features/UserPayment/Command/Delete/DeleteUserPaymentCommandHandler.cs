using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;

namespace OnlineShop.Application.Features.UserPayment.Command.Delete
{
    public class DeleteUserPaymentCommandHandler : IRequestHandler<DeleteUserPaymentCommand, Result<bool>>
    {
        private readonly IUserPaymentRepository _repository;

        public DeleteUserPaymentCommandHandler(IUserPaymentRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteUserPaymentCommand request, CancellationToken cancellationToken)
        {
            var userPayment = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (userPayment == null)
                return Result<bool>.Failure("UserPayment not found");

            await _repository.DeleteAsync(request.Id, cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
