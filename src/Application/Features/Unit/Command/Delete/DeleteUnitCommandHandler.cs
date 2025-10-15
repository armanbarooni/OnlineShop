using MediatR;
using OnlineShop.Application.Common.Models;

using System;
using System.Threading;
using System.Threading.Tasks;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Unit.Command.Delete
{
    public class DeleteUnitCommandHandler(IUnitRepository unitRepository) : IRequestHandler<DeleteUnitCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
        {

            if (request.Id == Guid.Empty)
                return Result<bool>.Failure("‘‰«”Â Ê«Õœ „⁄ »— ‰Ì” .");


            var entity = await unitRepository.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null)
                return Result<bool>.Failure("Ê«Õœ „Ê—œ ‰Ÿ— ÅÌœ« ‰‘œ.");

            entity.Delete(null);

            await unitRepository.UpdateAsync(entity, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}

