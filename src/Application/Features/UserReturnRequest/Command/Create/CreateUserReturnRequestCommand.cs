using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Command.Create
{
    public class CreateUserReturnRequestCommand : IRequest<Result<UserReturnRequestDto>>
    {
        public CreateUserReturnRequestDto? UserReturnRequest { get; set; }
    }
}


