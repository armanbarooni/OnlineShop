using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.MahakQueue;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.MahakQueue.Command.Create
{
    public class CreateMahakQueueCommandHandler(
        IMahakQueueRepository repository,
        IMapper mapper) : IRequestHandler<CreateMahakQueueCommand, Result<MahakQueueDto>>
    {
        public async Task<Result<MahakQueueDto>> Handle(CreateMahakQueueCommand request, CancellationToken cancellationToken)
        {
            if (request.MahakQueue == null)
                return Result<MahakQueueDto>.Failure("داده‌های صف محک نباید خالی باشد");

            try
            {
                var mahakQueue = Domain.Entities.MahakQueue.Create(
                    request.MahakQueue.QueueType,
                    request.MahakQueue.OperationType,
                    request.MahakQueue.EntityId,
                    request.MahakQueue.EntityType,
                    request.MahakQueue.Priority,
                    request.MahakQueue.MaxRetries,
                    request.MahakQueue.ScheduledAt,
                    request.MahakQueue.Payload
                );

                await repository.AddAsync(mahakQueue, cancellationToken);
                return Result<MahakQueueDto>.Success(mapper.Map<MahakQueueDto>(mahakQueue));
            }
            catch (Exception ex)
            {
                return Result<MahakQueueDto>.Failure($"خطا در ایجاد صف محک: {ex.Message}");
            }
        }
    }
}



