using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineShop.Application.Features.Unit.Command.Delete
{
    public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand, Result<bool>>
    {
        private readonly IUnitRepository _unitRepository;

        public DeleteUnitCommandHandler(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<Result<bool>> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
        {

            if (request.Id == Guid.Empty)
                return Result<bool>.Failure("شناسه واحد معتبر نیست.");


            var entity = await _unitRepository.GetByIdAsync(request.Id,cancellationToken);
            if (entity == null)
                return Result<bool>.Failure("واحد مورد نظر پیدا نشد.");

            entity.Delete(null);

            await _unitRepository.UpdateAsync(entity, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
