using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// Season entity for products (Spring, Summer, Fall, Winter, All-Season)
    /// </summary>
    public class Season : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Code { get; private set; } = string.Empty; // SPRING, SUMMER, FALL, WINTER, ALL
        public bool IsActive { get; private set; } = true;

        // Navigation Properties (many-to-many with Product)
        public virtual ICollection<ProductSeason> ProductSeasons { get; private set; } = new List<ProductSeason>();

        protected Season() { }

        private Season(string name, string code)
        {
            SetName(name);
            SetCode(code);
            IsActive = true;
            Deleted = false;
        }

        public static Season Create(string name, string code)
            => new(name, code);

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام فصل نباید خالی باشد");
            Name = name.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("کد فصل نباید خالی باشد");
            Code = code.Trim().ToUpper();
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string code, string? updatedBy)
        {
            SetName(name);
            SetCode(code);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این فصل قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

