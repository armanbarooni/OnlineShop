using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Application.Exceptions;
namespace OnlineShop.Application.Features.Unit.Queries.GetById
{
    public class GetUnitByIdQueryHandler : IRequestHandler<GetUnitByIdQuery, Result<UnitDetailsDto>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;

        public GetUnitByIdQueryHandler(IUnitRepository unitRepository, IMapper mapper)
        {
            _unitRepository = unitRepository;
            _mapper = mapper;
        }

        public async Task<Result<UnitDetailsDto>> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
        {

            var unit = await _unitRepository.GetByIdAsync(request.Id, cancellationToken);
            if (unit == null)
                throw new NotFoundException(nameof(Unit), request.Id);
            var res = _mapper.Map<UnitDetailsDto>(unit);
            return Result<UnitDetailsDto>.Success(res);
        }


    }
}