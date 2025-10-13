using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;

namespace OnlineShop.Application.Features.MahakSyncLog.Command.Delete
{
    public class DeleteMahakSyncLogCommandHandler(IMahakSyncLogRepository repository)
        : IRequestHandler<DeleteMahakSyncLogCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteMahakSyncLogCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var mahakSyncLog = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (mahakSyncLog == null)
                    return Result<bool>.Failure("لاگ سینک محک یافت نشد");

                mahakSyncLog.Delete(null);
                await repository.UpdateAsync(mahakSyncLog, cancellationToken);
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطا در حذف لاگ سینک محک: {ex.Message}");
            }
        }
    }
}

