using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;

namespace OnlineShop.Application.Features.Season.Commands.Delete
{
    public class DeleteSeasonCommandHandler : IRequestHandler<DeleteSeasonCommand, Result<bool>>
    {
        private readonly ISeasonRepository _repository;

        public DeleteSeasonCommandHandler(ISeasonRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteSeasonCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var season = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (season == null)
                {
                    return Result<bool>.Failure("فصل مورد نظر یافت نشد");
                }

                await _repository.DeleteAsync(request.Id, cancellationToken);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطا در حذف فصل: {ex.Message}");
            }
        }
    }
}
