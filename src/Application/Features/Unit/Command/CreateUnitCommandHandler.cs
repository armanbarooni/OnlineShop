using AutoMapper;
using MediatR;
using MySqlX.XDevAPI.Common;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence; 
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Unit;
using System.Xml.Linq;


namespace OnlineShop.Application.Features.Unit.Commands.CreateUnit
{
    public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, Result<bool>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        public CreateUnitCommandHandler(IUnitRepository unitRepository, IMapper mapper)
        {
            _unitRepository = unitRepository;
            _mapper = mapper;
        }

        public async Task<Result<bool>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
        {

            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<bool>.Failure("نام واحد نمی‌تواند خالی باشد.");

            // بررسی یکتا بودن
            var exists = await _unitRepository.ExistsByNameAsync(request.Name);
            if (exists)
                return Result<bool>.Failure("واحدی با این نام قبلاً ثبت شده است.");

            CreateUnitDto entity = new(request.Name,request.Comment);

            await _unitRepository.AddAsync(_mapper.Map<Unit>(entity));

            return Result<bool>.Success($"واحد '{request.Name}' با موفقیت ایجاد شد.");
        }
    }
}
