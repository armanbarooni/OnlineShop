using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Brand;

namespace OnlineShop.Application.Features.Brand.Commands.Create
{
    public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, Result<BrandDto>>
    {
        private readonly IBrandRepository _repository;
        private readonly IMapper _mapper;

        public CreateBrandCommandHandler(IBrandRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<BrandDto>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
        {
            // Check if brand with same name already exists
            var exists = await _repository.ExistsByNameAsync(request.Brand.Name, cancellationToken);
            if (exists)
                return Result<BrandDto>.Failure("برند با این نام قبلاً ثبت شده است");

            var brand = Domain.Entities.Brand.Create(
                request.Brand.Name,
                request.Brand.LogoUrl,
                request.Brand.Description
            );

            brand.SetWebsite(request.Brand.Website);
            brand.SetDisplayOrder(request.Brand.DisplayOrder);

            await _repository.AddAsync(brand, cancellationToken);

            var brandDto = _mapper.Map<BrandDto>(brand);
            return Result<BrandDto>.Success(brandDto);
        }
    }
}

