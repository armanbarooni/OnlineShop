using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.MahakQueue;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.MahakQueue.Queries.GetAll
{
    public class GetAllMahakQueuesQueryHandler(
        IMahakQueueRepository repository,
        IMapper mapper) : IRequestHandler<GetAllMahakQueuesQuery, Result<List<MahakQueueDto>>>
    {
        public async Task<Result<List<MahakQueueDto>>> Handle(GetAllMahakQueuesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var mahakQueues = await repository.GetAllAsync(cancellationToken);
                var dtoList = mapper.Map<List<MahakQueueDto>>(mahakQueues);
                return Result<List<MahakQueueDto>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                return Result<List<MahakQueueDto>>.Failure($"خطا در دریافت لیست صف‌های محک: {ex.Message}");
            }
        }
    }
}



