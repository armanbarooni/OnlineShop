using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.UserOrder.Queries.GenerateInvoice
{
    public class GenerateOrderInvoiceQuery : IRequest<Result<byte[]>>
    {
        public Guid OrderId { get; set; }
    }
}

