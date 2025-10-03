using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class Unit : BaseEntity
    {
        public int? UnitCode { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }

        protected Unit() { }

        private Unit(int unitCode, string name, long? mahakClientId, int? mahakId, string comment)
        {
            SetName(name);
            SetComment(comment);
            UnitCode = unitCode;
            MahakClientId = mahakClientId;
            MahakId = mahakId;
            Deleted = false;
        }

        public static Unit Create(int unitCode, string name, long? mahakClientId, int? mahakId, string comment)
            => new(unitCode, name, mahakClientId, mahakId, comment);

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام واحد نباید خالی باشد");
            Name = name.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetComment(string comment)
        {
            Comment = comment;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsDeleted()
        {
            if (Deleted) return;
            Deleted = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string comment, int? updateAt)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "نام واحد نمی‌تواند خالی باشد.");

            Name = name;
            Comment = comment;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updateAt ?? 1;
        }

        public void Delete(int? updateAt)
        {
            if (Deleted)
                throw new InvalidOperationException("این واحد قبلاً حذف شده است.");

            Deleted = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updateAt ?? 1;
        }
    }
}
