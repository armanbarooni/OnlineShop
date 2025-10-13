using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Command.Reject
{
    public class RejectUserReturnRequestCommand : IRequest<Result<UserReturnRequestDto>>
    {
        public Guid Id { get; set; }
        public string? RejectionReason { get; set; }
        public string? AdminNotes { get; set; }
    }
}

