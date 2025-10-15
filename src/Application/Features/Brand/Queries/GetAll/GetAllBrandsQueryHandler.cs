using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.Brand;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Brand.Queries.GetAll
{
    public class GetAllBrandsQueryHandler : IRequestHandler<GetAllBrandsQuery, Result<List<BrandDto>>>
    {
        private readonly IBrandRepository _repository;
        private readonly IMapper _mapper;

        public GetAllBrandsQueryHandler(IBrandRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<List<BrandDto>>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
        {
            var brands = await _repository.GetAllAsync(cancellationToken);
            var brandDtos = _mapper.Map<List<BrandDto>>(brands);
            return Result<List<BrandDto>>.Success(brandDtos);
        }
    }
}



