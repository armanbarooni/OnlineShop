using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Brand;

namespace OnlineShop.Application.Features.Brand.Queries.GetAll
{
    public class GetAllBrandsQuery : IRequest<Result<List<BrandDto>>>
    {
    }
}


