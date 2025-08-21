using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Domain.Common
{
    public abstract class BaseEntity
    {
        public int? MahakId { get; set; }
        public long? MahakClientId { get; set; }
        public long RowVersion { get; set; }
        public bool Deleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
