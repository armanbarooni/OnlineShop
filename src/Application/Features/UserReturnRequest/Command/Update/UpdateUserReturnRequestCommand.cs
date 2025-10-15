using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Command.Update
{
    public class UpdateUserReturnRequestCommand : IRequest<Result<UserReturnRequestDto>>
    {
        public UpdateUserReturnRequestDto? UserReturnRequest { get; set; }
    }
}


