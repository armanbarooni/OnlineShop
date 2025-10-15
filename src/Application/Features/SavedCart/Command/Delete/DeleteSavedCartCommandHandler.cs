using MediatR;
using OnlineShop.Application.Common.Models;


using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.SavedCart.Command.Delete
{
    public class DeleteSavedCartCommandHandler : IRequestHandler<DeleteSavedCartCommand, Result<bool>>
    {
        private readonly ISavedCartRepository _repository;

        public DeleteSavedCartCommandHandler(ISavedCartRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteSavedCartCommand request, CancellationToken cancellationToken)
        {
            var savedCart = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (savedCart == null)
                return Result<bool>.Failure("SavedCart not found");

            await _repository.DeleteAsync(request.Id, cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}


