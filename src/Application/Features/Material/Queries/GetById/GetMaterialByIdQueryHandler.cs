using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Material;

namespace OnlineShop.Application.Features.Material.Queries.GetById
{
    public class GetMaterialByIdQueryHandler : IRequestHandler<GetMaterialByIdQuery, Result<MaterialDto>>
    {
        private readonly IMaterialRepository _repository;
        private readonly IMapper _mapper;

        public GetMaterialByIdQueryHandler(IMaterialRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<MaterialDto>> Handle(GetMaterialByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var material = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (material == null)
                {
                    return Result<MaterialDto>.Failure("متریال مورد نظر یافت نشد");
                }

                var materialDto = _mapper.Map<MaterialDto>(material);
                return Result<MaterialDto>.Success(materialDto);
            }
            catch (Exception ex)
            {
                return Result<MaterialDto>.Failure($"خطا در دریافت متریال: {ex.Message}");
            }
        }
    }
}
