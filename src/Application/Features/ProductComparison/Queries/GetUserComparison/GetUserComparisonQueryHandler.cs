using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductComparison;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductComparison.Queries.GetUserComparison
{
    public class GetUserComparisonQueryHandler : IRequestHandler<GetUserComparisonQuery, Result<ProductComparisonDto?>>
    {
        private readonly IProductComparisonRepository _repository;
        private readonly IMapper _mapper;

        public GetUserComparisonQueryHandler(IProductComparisonRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductComparisonDto?>> Handle(GetUserComparisonQuery request, CancellationToken cancellationToken)
        {
            var comparison = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (comparison == null)
                return Result<ProductComparisonDto?>.Success(null);

            var dto = _mapper.Map<ProductComparisonDto>(comparison);
            return Result<ProductComparisonDto?>.Success(dto);
        }
    }
}

