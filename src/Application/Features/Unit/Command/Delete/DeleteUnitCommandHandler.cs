using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineShop.Application.Features.Unit.Command.Delete
{
    public class DeleteUnitCommandHandler(IUnitRepository unitRepository) : IRequestHandler<DeleteUnitCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
        {

            if (request.Id == Guid.Empty)
                return Result<bool>.Failure("شناسه واحد معتبر نیست.");


            var entity = await unitRepository.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null)
                return Result<bool>.Failure("واحد مورد نظر پیدا نشد.");

            entity.Delete(null);

            await unitRepository.UpdateAsync(entity, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}