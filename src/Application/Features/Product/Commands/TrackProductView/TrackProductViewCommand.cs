using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserProductView;

namespace OnlineShop.Application.Features.Product.Commands.TrackProductView
{
    public class TrackProductViewCommand : IRequest<Result<bool>>
    {
        public string UserId { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public string? SessionId { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
    }
}

