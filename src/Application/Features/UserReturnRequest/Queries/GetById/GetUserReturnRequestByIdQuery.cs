using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Queries.GetById
{
    public class GetUserReturnRequestByIdQuery : IRequest<Result<UserReturnRequestDto>>
    {
        public Guid Id { get; set; }
    }
}

