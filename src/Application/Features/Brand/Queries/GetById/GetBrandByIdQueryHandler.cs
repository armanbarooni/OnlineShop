using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.Brand;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Brand.Queries.GetById
{
    public class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, Result<BrandDto>>
    {
        private readonly IBrandRepository _repository;
        private readonly IMapper _mapper;

        public GetBrandByIdQueryHandler(IBrandRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<BrandDto>> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
        {
            var brand = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (brand == null)
                return Result<BrandDto>.Failure("برند یافت نشد");

            var brandDto = _mapper.Map<BrandDto>(brand);
            return Result<BrandDto>.Success(brandDto);
        }
    }
}



