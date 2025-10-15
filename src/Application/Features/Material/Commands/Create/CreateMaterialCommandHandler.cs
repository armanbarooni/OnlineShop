using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Material;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Features.Material.Commands.Create
{
    public class CreateMaterialCommandHandler : IRequestHandler<CreateMaterialCommand, Result<MaterialDto>>
    {
        private readonly IMaterialRepository _repository;
        private readonly IMapper _mapper;

        public CreateMaterialCommandHandler(IMaterialRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<MaterialDto>> Handle(CreateMaterialCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if material with same name already exists
                var existingMaterial = await _repository.ExistsByNameAsync(request.Material.Name, cancellationToken);
                if (existingMaterial)
                {
                    return Result<MaterialDto>.Failure("متریال با این نام قبلاً وجود دارد");
                }

                // Create new material
                var material = Domain.Entities.Material.Create(request.Material.Name, request.Material.Description);
                
                await _repository.AddAsync(material, cancellationToken);

                var materialDto = _mapper.Map<MaterialDto>(material);
                return Result<MaterialDto>.Success(materialDto);
            }
            catch (Exception ex)
            {
                return Result<MaterialDto>.Failure($"خطا در ایجاد متریال: {ex.Message}");
            }
        }
    }
}
