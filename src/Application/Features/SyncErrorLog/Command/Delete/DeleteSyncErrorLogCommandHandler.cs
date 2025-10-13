using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;

namespace OnlineShop.Application.Features.SyncErrorLog.Command.Delete
{
    public class DeleteSyncErrorLogCommandHandler(ISyncErrorLogRepository repository)
        : IRequestHandler<DeleteSyncErrorLogCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteSyncErrorLogCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var syncErrorLog = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (syncErrorLog == null)
                    return Result<bool>.Failure("لاگ خطا یافت نشد");

                syncErrorLog.Delete(null);
                await repository.UpdateAsync(syncErrorLog, cancellationToken);
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطا در حذف لاگ خطا: {ex.Message}");
            }
        }
    }
}

