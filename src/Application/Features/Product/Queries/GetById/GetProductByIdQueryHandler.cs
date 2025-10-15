using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Exceptions;
using OnlineShop.Application.Features.Product.Commands.TrackProductView;
using System.Security.Claims;

namespace OnlineShop.Application.Features.Product.Queries.GetById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDetailsDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetProductByIdQueryHandler(
            IProductRepository productRepository, 
            IMapper mapper,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<ProductDetailsDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdWithIncludesAsync(request.Id, cancellationToken);
            if (product == null)
            {
                return Result<ProductDetailsDto>.Failure($"Product with ID {request.Id} not found");
            }

            // Track product view if user is authenticated
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                // Fire and forget - don't wait for tracking to complete
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
                        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
                        
                        await _mediator.Send(new TrackProductViewCommand
                        {
                            UserId = userId,
                            ProductId = request.Id,
                            UserAgent = userAgent,
                            IpAddress = ipAddress
                        }, CancellationToken.None);
                    }
                    catch
                    {
                        // Ignore tracking errors - don't affect main functionality
                    }
                });
            }

            var dto = _mapper.Map<ProductDetailsDto>(product);
            return Result<ProductDetailsDto>.Success(dto);
        }
    }
}
