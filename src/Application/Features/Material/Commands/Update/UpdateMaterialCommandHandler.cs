using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.Material;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Material.Commands.Update
{
    public class UpdateMaterialCommandHandler : IRequestHandler<UpdateMaterialCommand, Result<MaterialDto>>
    {
        private readonly IMaterialRepository _repository;
        private readonly IMapper _mapper;

        public UpdateMaterialCommandHandler(IMaterialRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<MaterialDto>> Handle(UpdateMaterialCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var material = await _repository.GetByIdAsync(request.Material.Id, cancellationToken);
                if (material == null)
                {
                    return Result<MaterialDto>.Failure("متریال مورد نظر یافت نشد");
                }

                // Check if another material with same name exists (excluding current one)
                var existingMaterial = await _repository.ExistsByNameAsync(request.Material.Name, cancellationToken);
                if (existingMaterial)
                {
                    // Get the existing material to check if it's the same one
                    var allMaterials = await _repository.GetAllAsync(cancellationToken);
                    var materialWithSameName = allMaterials.FirstOrDefault(m => m.Name == request.Material.Name && m.Id != request.Material.Id);
                    if (materialWithSameName != null)
                    {
                        return Result<MaterialDto>.Failure("متریال با این نام قبلاً وجود دارد");
                    }
                }

                // Update material
                material.Update(request.Material.Name, request.Material.Description, "System");
                
                await _repository.UpdateAsync(material, cancellationToken);

                var materialDto = _mapper.Map<MaterialDto>(material);
                return Result<MaterialDto>.Success(materialDto);
            }
            catch (Exception ex)
            {
                return Result<MaterialDto>.Failure($"خطا در به‌روزرسانی متریال: {ex.Message}");
            }
        }
    }
}


