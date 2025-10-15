using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int? MahakId { get; set; }
        public long? MahakClientId { get; set; }
        public long RowVersion { get; set; }
        public bool Deleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }

        public virtual void Delete(string? deletedBy = null)
        {
            if (Deleted)
                throw new InvalidOperationException("این رکورد قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = deletedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
