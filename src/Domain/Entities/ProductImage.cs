using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public string ImageUrl { get; private set; } = string.Empty;
        public string? AltText { get; private set; }
        public string? Title { get; private set; }
        public int DisplayOrder { get; private set; }
        public bool IsPrimary { get; private set; }
        public string ImageType { get; private set; } = "Main"; // Main, Hover, Gallery, 360, Video
        public long FileSize { get; private set; }
        public string? MimeType { get; private set; }

        // Navigation Properties
        public virtual Product Product { get; private set; } = null!;

        protected ProductImage() { }

        private ProductImage(Guid productId, string imageUrl, string? altText, string? title, 
            int displayOrder, bool isPrimary, string imageType, long fileSize, string? mimeType)
        {
            ProductId = productId;
            SetImageUrl(imageUrl);
            SetAltText(altText);
            SetTitle(title);
            SetDisplayOrder(displayOrder);
            SetIsPrimary(isPrimary);
            SetImageType(imageType);
            SetFileSize(fileSize);
            SetMimeType(mimeType);
            Deleted = false;
        }

        public static ProductImage Create(Guid productId, string imageUrl, string? altText = null, 
            string? title = null, int displayOrder = 0, bool isPrimary = false, 
            string imageType = "Main", long fileSize = 0, string? mimeType = null)
            => new(productId, imageUrl, altText, title, displayOrder, isPrimary, imageType, fileSize, mimeType);

        public void SetImageUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("آدرس تصویر محصول نباید خالی باشد");
            ImageUrl = imageUrl.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAltText(string? altText)
        {
            AltText = altText?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetTitle(string? title)
        {
            Title = title?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDisplayOrder(int displayOrder)
        {
            if (displayOrder < 0)
                throw new ArgumentException("ترتیب نمایش نمی‌تواند منفی باشد");
            DisplayOrder = displayOrder;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetIsPrimary(bool isPrimary)
        {
            IsPrimary = isPrimary;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetFileSize(long fileSize)
        {
            if (fileSize < 0)
                throw new ArgumentException("اندازه فایل نمی‌تواند منفی باشد");
            FileSize = fileSize;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetMimeType(string? mimeType)
        {
            MimeType = mimeType?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetImageType(string imageType)
        {
            var validTypes = new[] { "Main", "Hover", "Gallery", "360", "Video" };
            if (!validTypes.Contains(imageType, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("نوع تصویر باید یکی از مقادیر Main, Hover, Gallery, 360, Video باشد");
            ImageType = imageType.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string imageUrl, string? altText, string? title, int displayOrder, 
            bool isPrimary, string imageType, long fileSize, string? mimeType, string? updatedBy)
        {
            SetImageUrl(imageUrl);
            SetAltText(altText);
            SetTitle(title);
            SetDisplayOrder(displayOrder);
            SetIsPrimary(isPrimary);
            SetImageType(imageType);
            SetFileSize(fileSize);
            SetMimeType(mimeType);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این تصویر محصول قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
