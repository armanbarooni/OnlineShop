using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Unit.Command.Update
{
    public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, Result<bool>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;

        public UpdateUnitCommandHandler(IUnitRepository unitRepository, IMapper mapper)
        {
            _unitRepository = unitRepository;
            _mapper = mapper;
        }

        public async Task<Result<bool>> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
        {

            if (request.UnitDto == null)
                return Result<bool>.Failure("œ—ŒÊ«”  ‰„Ìù Ê«‰œ Œ«·Ì »«‘œ.");

            if (request.UnitDto.Id == Guid.Empty)
                return Result<bool>.Failure("‘‰«”Â Ê«Õœ „⁄ »— ‰Ì” .");

            if (string.IsNullOrWhiteSpace(request.UnitDto.Name))
                return Result<bool>.Failure("‰«„ Ê«Õœ ‰„Ìù Ê«‰œ Œ«·Ì »«‘œ.");

            var entity = await _unitRepository.GetByIdAsync(request.UnitDto.Id, cancellationToken);
            if (entity == null)
                return Result<bool>.Failure("Ê«Õœ „Ê—œ ‰Ÿ— ÅÌœ« ‰‘œ.");

            var nameExists = await _unitRepository.ExistsByNameAsync(request.UnitDto.Name, cancellationToken);
            if (nameExists)
                return Result<bool>.Failure("Ê«ÕœÌ »« «Ì‰ ‰«„ ﬁ»·« À»  ‘œÂ «” .");


            entity.Update(request.UnitDto.Name, request.UnitDto.Comment, null);
            await _unitRepository.UpdateAsync(entity, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}

