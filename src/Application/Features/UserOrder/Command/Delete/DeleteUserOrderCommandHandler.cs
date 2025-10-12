using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;

namespace OnlineShop.Application.Features.UserOrder.Command.Delete
{
    public class DeleteUserOrderCommandHandler : IRequestHandler<DeleteUserOrderCommand, Result<bool>>
    {
        private readonly IUserOrderRepository _repository;

        public DeleteUserOrderCommandHandler(IUserOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteUserOrderCommand request, CancellationToken cancellationToken)
        {
            var userOrder = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (userOrder == null)
                return Result<bool>.Failure("UserOrder not found");

            await _repository.DeleteAsync(request.Id, cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
