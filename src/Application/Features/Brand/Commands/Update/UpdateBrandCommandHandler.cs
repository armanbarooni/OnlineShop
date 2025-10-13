using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Brand;

namespace OnlineShop.Application.Features.Brand.Commands.Update
{
    public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, Result<BrandDto>>
    {
        private readonly IBrandRepository _repository;
        private readonly IMapper _mapper;

        public UpdateBrandCommandHandler(IBrandRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<BrandDto>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
        {
            var brand = await _repository.GetByIdAsync(request.Brand.Id, cancellationToken);
            if (brand == null)
                return Result<BrandDto>.Failure("برند یافت نشد");

            brand.Update(
                request.Brand.Name,
                request.Brand.LogoUrl,
                request.Brand.Description,
                request.Brand.Website,
                request.Brand.DisplayOrder,
                null
            );

            if (request.Brand.IsActive)
                brand.Activate();
            else
                brand.Deactivate();

            await _repository.UpdateAsync(brand, cancellationToken);

            var brandDto = _mapper.Map<BrandDto>(brand);
            return Result<BrandDto>.Success(brandDto);
        }
    }
}

