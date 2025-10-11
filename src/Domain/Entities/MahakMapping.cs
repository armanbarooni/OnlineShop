using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class MahakMapping : BaseEntity
    {
        public string EntityType { get; private set; } = string.Empty;
        public Guid LocalEntityId { get; private set; }
        public int MahakEntityId { get; private set; }
        public string? MahakEntityCode { get; private set; }
        public string MappingStatus { get; private set; } = "Active";
        public DateTime MappedAt { get; private set; }
        public DateTime? UnmappedAt { get; private set; }
        public string? UnmappedReason { get; private set; }
        public string? Notes { get; private set; }

        protected MahakMapping() { }

        private MahakMapping(string entityType, Guid localEntityId, int mahakEntityId, 
            string? mahakEntityCode, string? notes)
        {
            SetEntityType(entityType);
            LocalEntityId = localEntityId;
            SetMahakEntityId(mahakEntityId);
            SetMahakEntityCode(mahakEntityCode);
            SetNotes(notes);
            MappingStatus = "Active";
            MappedAt = DateTime.UtcNow;
            Deleted = false;
        }

        public static MahakMapping Create(string entityType, Guid localEntityId, int mahakEntityId, 
            string? mahakEntityCode = null, string? notes = null)
            => new(entityType, localEntityId, mahakEntityId, mahakEntityCode, notes);

        public void SetEntityType(string entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("نوع موجودیت نباید خالی باشد");
            EntityType = entityType.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetMahakEntityId(int mahakEntityId)
        {
            if (mahakEntityId <= 0)
                throw new ArgumentException("شناسه موجودیت محک باید بزرگتر از صفر باشد");
            MahakEntityId = mahakEntityId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetMahakEntityCode(string? mahakEntityCode)
        {
            MahakEntityCode = mahakEntityCode?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetNotes(string? notes)
        {
            Notes = notes?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void Unmap(string unmappedReason, string? notes = null)
        {
            MappingStatus = "Inactive";
            UnmappedAt = DateTime.UtcNow;
            UnmappedReason = unmappedReason?.Trim();
            SetNotes(notes);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reactivate(string? notes = null)
        {
            MappingStatus = "Active";
            UnmappedAt = null;
            UnmappedReason = null;
            SetNotes(notes);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(int mahakEntityId, string? mahakEntityCode, string? notes, string? updatedBy)
        {
            SetMahakEntityId(mahakEntityId);
            SetMahakEntityCode(mahakEntityCode);
            SetNotes(notes);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این نگاشت قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
