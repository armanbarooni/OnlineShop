using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Command.Approve
{
    public class ApproveUserReturnRequestCommand : IRequest<Result<UserReturnRequestDto>>
    {
        public Guid Id { get; set; }
        public string? AdminNotes { get; set; }
    }
}

