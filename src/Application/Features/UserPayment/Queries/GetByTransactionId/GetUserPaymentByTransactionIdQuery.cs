using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;

namespace OnlineShop.Application.Features.UserPayment.Queries.GetByTransactionId
{
    public class GetUserPaymentByTransactionIdQuery : IRequest<Result<UserPaymentDto?>>
    {
        public string TransactionId { get; set; } = string.Empty;
    }
}

