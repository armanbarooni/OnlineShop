using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductCategory.Queries.GetCategoryTree
{
    public class GetCategoryTreeQueryHandler : IRequestHandler<GetCategoryTreeQuery, Result<List<ProductCategoryDto>>>
    {
        private readonly IProductCategoryRepository _repository;
        private readonly IMapper _mapper;

        public GetCategoryTreeQueryHandler(IProductCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductCategoryDto>>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
        {
            var categories = await _repository.GetCategoryTreeAsync(cancellationToken);
            var categoryDtos = _mapper.Map<List<ProductCategoryDto>>(categories);
            return Result<List<ProductCategoryDto>>.Success(categoryDtos);
        }
    }
}

