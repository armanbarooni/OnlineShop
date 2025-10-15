using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.Unit;
using AutoMapper;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Unit.Queries.GetAll
{
    public class GetAllUnitsQueryHandler
        : IRequestHandler<GetAllUnitsQuery, Result<List<UnitDto>>>
    {
        private readonly IUnitRepository _repository;
        private readonly IMapper _mapper;

        public GetAllUnitsQueryHandler(IUnitRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<List<UnitDto>>> Handle(
            GetAllUnitsQuery request,
            CancellationToken cancellationToken)
        {
            var units = await _repository.GetAllAsync(cancellationToken);
            var dtos = _mapper.Map<List<UnitDto>>(units);
            return Result<List<UnitDto>>.Success(dtos);
        }
    }
}

