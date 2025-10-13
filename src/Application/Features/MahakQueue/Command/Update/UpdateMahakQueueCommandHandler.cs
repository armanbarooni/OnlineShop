using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.MahakQueue;

namespace OnlineShop.Application.Features.MahakQueue.Command.Update
{
    public class UpdateMahakQueueCommandHandler(
        IMahakQueueRepository repository,
        IMapper mapper) : IRequestHandler<UpdateMahakQueueCommand, Result<MahakQueueDto>>
    {
        public async Task<Result<MahakQueueDto>> Handle(UpdateMahakQueueCommand request, CancellationToken cancellationToken)
        {
            if (request.MahakQueue == null)
                return Result<MahakQueueDto>.Failure("داده‌های صف محک نباید خالی باشد");

            try
            {
                var mahakQueue = await repository.GetByIdAsync(request.MahakQueue.Id, cancellationToken);
                
                if (mahakQueue == null)
                    return Result<MahakQueueDto>.Failure("صف محک یافت نشد");

                mahakQueue.Update(
                    request.MahakQueue.QueueType,
                    request.MahakQueue.OperationType,
                    request.MahakQueue.EntityType,
                    request.MahakQueue.Priority,
                    request.MahakQueue.Payload,
                    request.MahakQueue.UpdatedBy
                );

                await repository.UpdateAsync(mahakQueue, cancellationToken);
                return Result<MahakQueueDto>.Success(mapper.Map<MahakQueueDto>(mahakQueue));
            }
            catch (Exception ex)
            {
                return Result<MahakQueueDto>.Failure($"خطا در به‌روزرسانی صف محک: {ex.Message}");
            }
        }
    }
}

