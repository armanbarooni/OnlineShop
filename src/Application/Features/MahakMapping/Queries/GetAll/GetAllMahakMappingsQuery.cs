using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.MahakMapping;

namespace OnlineShop.Application.Features.MahakMapping.Queries.GetAll
{
    public class GetAllMahakMappingsQuery : IRequest<Result<List<MahakMappingDto>>>
    {
    }
}


