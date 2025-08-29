using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entites;
using System;
using System.Threading;
using System.Threading.Tasks;

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
                return Result<bool>.Failure("درخواست نمی‌تواند خالی باشد.");

            if (request.UnitDto.Id == Guid.Empty)
                return Result<bool>.Failure("شناسه واحد معتبر نیست.");

            if (string.IsNullOrWhiteSpace(request.UnitDto.Name))
                return Result<bool>.Failure("نام واحد نمی‌تواند خالی باشد.");

            var entity = await _unitRepository.GetByIdAsync(request.UnitDto.Id,cancellationToken);
            if (entity == null)
                return Result<bool>.Failure("واحد مورد نظر پیدا نشد.");

            var nameExists = await _unitRepository.ExistsByNameAsync(request.UnitDto.Name, cancellationToken);
            if (nameExists)
                return Result<bool>.Failure("واحدی با این نام قبلاً ثبت شده است.");


            entity.Update(request.UnitDto.Name, request.UnitDto.Comment,null);
            await _unitRepository.UpdateAsync(entity, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
