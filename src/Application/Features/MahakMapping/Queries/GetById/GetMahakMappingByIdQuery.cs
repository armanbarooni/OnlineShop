using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.MahakMapping;

namespace OnlineShop.Application.Features.MahakMapping.Queries.GetById
{
    public class GetMahakMappingByIdQuery : IRequest<Result<MahakMappingDto>>
    {
        public Guid Id { get; set; }
    }
}

