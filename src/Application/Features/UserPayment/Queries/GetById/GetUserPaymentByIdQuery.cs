using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;

namespace OnlineShop.Application.Features.UserPayment.Queries.GetById
{
    public class GetUserPaymentByIdQuery : IRequest<Result<UserPaymentDto>>
    {
        public Guid Id { get; set; }
    }
}

