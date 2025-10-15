using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductVariant;

namespace OnlineShop.Application.Features.ProductVariant.Queries.GetAll
{
    public class GetAllProductVariantsQueryHandler : IRequestHandler<GetAllProductVariantsQuery, Result<List<ProductVariantDto>>>
    {
        private readonly IProductVariantRepository _repository;
        private readonly IMapper _mapper;

        public GetAllProductVariantsQueryHandler(IProductVariantRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductVariantDto>>> Handle(GetAllProductVariantsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var productVariants = await _repository.GetAllAsync(cancellationToken);
                var productVariantDtos = _mapper.Map<List<ProductVariantDto>>(productVariants);
                return Result<List<ProductVariantDto>>.Success(productVariantDtos);
            }
            catch (Exception ex)
            {
                return Result<List<ProductVariantDto>>.Failure($"خطا در دریافت لیست تنوع‌های محصول: {ex.Message}");
            }
        }
    }
}
