using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Unit.Command.Create
{
    public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, Result<Guid>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;

        public CreateUnitCommandHandler(IUnitRepository unitRepository, IMapper mapper)
        {
            _unitRepository = unitRepository;
            _mapper = mapper;
        }

        public async Task<Result<Guid>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
        {
            if (request.UnitDto == null)
                return Result<Guid>.Failure("������� ��� ����� ���� ����");

            if (string.IsNullOrWhiteSpace(request.UnitDto.Name))
                return Result<Guid>.Failure("��� ���� �������� ���� ����.");

            var exists = await _unitRepository.ExistsByNameAsync(request.UnitDto.Name, cancellationToken);
            if (exists)
                return Result<Guid>.Failure("����� �� ��� ��� ����� ��� ��� ���.");


            var result = _mapper.Map<Domain.Entities.Unit>(request.UnitDto);

            await _unitRepository.AddAsync(result, cancellationToken);
            return Result<Guid>.Success(result.Id);
        }
    }
}

