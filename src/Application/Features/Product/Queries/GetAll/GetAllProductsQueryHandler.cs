using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Product;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineShop.Application.Features.Product.Queries.GetAll
{
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<List<ProductDto>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetAllProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetAllAsync(cancellationToken);
            var dtoList = _mapper.Map<List<ProductDto>>(products);
            return Result<List<ProductDto>>.Success(dtoList);
        }
    }
}
