using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;

namespace OnlineShop.Application.Features.MahakQueue.Command.Delete
{
    public class DeleteMahakQueueCommandHandler(IMahakQueueRepository repository)
        : IRequestHandler<DeleteMahakQueueCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteMahakQueueCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var mahakQueue = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (mahakQueue == null)
                    return Result<bool>.Failure("صف محک یافت نشد");

                mahakQueue.Delete(null);
                await repository.UpdateAsync(mahakQueue, cancellationToken);
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطا در حذف صف محک: {ex.Message}");
            }
        }
    }
}

