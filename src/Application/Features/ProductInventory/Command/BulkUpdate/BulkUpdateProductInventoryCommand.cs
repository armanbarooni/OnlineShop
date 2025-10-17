using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.ProductInventory.Command.BulkUpdate
{
    public class BulkUpdateProductInventoryCommand : IRequest<Result<bool>>
    {
        public List<BulkUpdateInventoryItem> Items { get; set; } = new();
    }

    public class BulkUpdateInventoryItem
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

