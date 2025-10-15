using MediatR;
using OnlineShop.Application.Common.Models;


using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Brand.Commands.Delete
{
    public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand, Result<bool>>
    {
        private readonly IBrandRepository _repository;

        public DeleteBrandCommandHandler(IBrandRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
        {
            var brand = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (brand == null)
                return Result<bool>.Failure("برند یافت نشد");

            await _repository.DeleteAsync(request.Id, cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}



