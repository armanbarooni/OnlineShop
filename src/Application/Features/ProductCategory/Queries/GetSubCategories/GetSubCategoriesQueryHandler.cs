using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductCategory.Queries.GetSubCategories
{
    public class GetSubCategoriesQueryHandler : IRequestHandler<GetSubCategoriesQuery, Result<List<ProductCategoryDto>>>
    {
        private readonly IProductCategoryRepository _repository;
        private readonly IMapper _mapper;

        public GetSubCategoriesQueryHandler(IProductCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductCategoryDto>>> Handle(GetSubCategoriesQuery request, CancellationToken cancellationToken)
        {
            var subCategories = await _repository.GetSubCategoriesAsync(request.ParentId, cancellationToken);
            var dtos = _mapper.Map<List<ProductCategoryDto>>(subCategories);
            return Result<List<ProductCategoryDto>>.Success(dtos);
        }
    }
}

