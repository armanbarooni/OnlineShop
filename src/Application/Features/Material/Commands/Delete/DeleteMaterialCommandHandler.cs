using MediatR;
using OnlineShop.Application.Common.Models;


using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Material.Commands.Delete
{
    public class DeleteMaterialCommandHandler : IRequestHandler<DeleteMaterialCommand, Result<bool>>
    {
        private readonly IMaterialRepository _repository;

        public DeleteMaterialCommandHandler(IMaterialRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteMaterialCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var material = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (material == null)
                {
                    return Result<bool>.Failure("متریال مورد نظر یافت نشد");
                }

                await _repository.DeleteAsync(request.Id, cancellationToken);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطا در حذف متریال: {ex.Message}");
            }
        }
    }
}


