using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.Material;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Material.Queries.GetAll
{
    public class GetAllMaterialsQueryHandler : IRequestHandler<GetAllMaterialsQuery, Result<List<MaterialDto>>>
    {
        private readonly IMaterialRepository _repository;
        private readonly IMapper _mapper;

        public GetAllMaterialsQueryHandler(IMaterialRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<List<MaterialDto>>> Handle(GetAllMaterialsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var materials = await _repository.GetAllAsync(cancellationToken);
                var materialDtos = _mapper.Map<List<MaterialDto>>(materials);
                return Result<List<MaterialDto>>.Success(materialDtos);
            }
            catch (Exception ex)
            {
                return Result<List<MaterialDto>>.Failure($"خطا در دریافت لیست متریال‌ها: {ex.Message}");
            }
        }
    }
}


