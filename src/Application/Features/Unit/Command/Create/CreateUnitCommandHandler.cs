using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Domain.Entities; // یا Domain.Entites - هر کدوم درسته

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
                return Result<Guid>.Failure("درخواست نمی تواند خالی باشد");

            if (string.IsNullOrWhiteSpace(request.UnitDto.Name))
                return Result<Guid>.Failure("نام واحد نمی‌تواند خالی باشد.");

            var exists = await _unitRepository.ExistsByNameAsync(request.UnitDto.Name, cancellationToken);
            if (exists)
                return Result<Guid>.Failure("واحدی با این نام قبلاً ثبت شده است.");


            var result = _mapper.Map<Domain.Entities.Unit>(request.UnitDto);

            await _unitRepository.AddAsync(result, cancellationToken);
            return Result<Guid>.Success(result.Id);
        }
    }
}
