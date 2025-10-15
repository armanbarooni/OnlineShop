using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductVariant;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.ProductVariant.Queries.GetByProductId
{
    public class GetProductVariantsByProductIdQueryHandler : IRequestHandler<GetProductVariantsByProductIdQuery, Result<List<ProductVariantDto>>>
    {
        private readonly IProductVariantRepository _repository;
        private readonly IMapper _mapper;

        public GetProductVariantsByProductIdQueryHandler(IProductVariantRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductVariantDto>>> Handle(GetProductVariantsByProductIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var productVariants = await _repository.GetByProductIdAsync(request.ProductId, cancellationToken);
                var productVariantDtos = _mapper.Map<List<ProductVariantDto>>(productVariants);
                return Result<List<ProductVariantDto>>.Success(productVariantDtos);
            }
            catch (Exception ex)
            {
                return Result<List<ProductVariantDto>>.Failure($"خطا در دریافت تنوع‌های محصول: {ex.Message}");
            }
        }
    }
}


