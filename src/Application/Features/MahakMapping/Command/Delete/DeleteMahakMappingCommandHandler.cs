using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;

namespace OnlineShop.Application.Features.MahakMapping.Command.Delete
{
    public class DeleteMahakMappingCommandHandler(IMahakMappingRepository repository)
        : IRequestHandler<DeleteMahakMappingCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteMahakMappingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var mahakMapping = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (mahakMapping == null)
                    return Result<bool>.Failure("نگاشت محک یافت نشد");

                mahakMapping.Delete(null);
                await repository.UpdateAsync(mahakMapping, cancellationToken);
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطا در حذف نگاشت محک: {ex.Message}");
            }
        }
    }
}

