using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductVariant;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.ProductVariant.Queries.GetById
{
    public class GetProductVariantByIdQueryHandler : IRequestHandler<GetProductVariantByIdQuery, Result<ProductVariantDto>>
    {
        private readonly IProductVariantRepository _repository;
        private readonly IMapper _mapper;

        public GetProductVariantByIdQueryHandler(IProductVariantRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductVariantDto>> Handle(GetProductVariantByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var productVariant = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (productVariant == null)
                {
                    return Result<ProductVariantDto>.Failure("تنوع محصول مورد نظر یافت نشد");
                }

                var productVariantDto = _mapper.Map<ProductVariantDto>(productVariant);
                return Result<ProductVariantDto>.Success(productVariantDto);
            }
            catch (Exception ex)
            {
                return Result<ProductVariantDto>.Failure($"خطا در دریافت تنوع محصول: {ex.Message}");
            }
        }
    }
}


