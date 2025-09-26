using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineShop.Application.Features.Product.Command.Create
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;

        public CreateProductCommandHandler(IProductRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Product>(request.Product);
            await _repository.AddAsync(product);
            return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
        }
    }
}
