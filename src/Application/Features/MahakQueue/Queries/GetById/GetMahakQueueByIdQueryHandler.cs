using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.MahakQueue;

namespace OnlineShop.Application.Features.MahakQueue.Queries.GetById
{
    public class GetMahakQueueByIdQueryHandler(
        IMahakQueueRepository repository,
        IMapper mapper) : IRequestHandler<GetMahakQueueByIdQuery, Result<MahakQueueDto>>
    {
        public async Task<Result<MahakQueueDto>> Handle(GetMahakQueueByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var mahakQueue = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (mahakQueue == null)
                    return Result<MahakQueueDto>.Failure("صف محک یافت نشد");

                var dto = mapper.Map<MahakQueueDto>(mahakQueue);
                return Result<MahakQueueDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<MahakQueueDto>.Failure($"خطا در دریافت صف محک: {ex.Message}");
            }
        }
    }
}

