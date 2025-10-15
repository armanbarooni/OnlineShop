using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;

namespace OnlineShop.Application.Features.UserPayment.Queries.GetAll
{
    public class GetAllUserPaymentsQuery : IRequest<Result<IEnumerable<UserPaymentDto>>>
    {
    }
}

