using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class ProductReview : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public Guid UserId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Comment { get; private set; } = string.Empty;
        public int Rating { get; private set; }
        public bool IsVerified { get; private set; }
        public bool IsApproved { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public string? ApprovedBy { get; private set; }
        public string? AdminNotes { get; private set; }
        public DateTime? RejectedAt { get; private set; }
        public string? RejectedBy { get; private set; }
        public string? RejectionReason { get; private set; }

        // Navigation Properties
        public virtual Product Product { get; private set; } = null!;
        public virtual ApplicationUser User { get; private set; } = null!;

        protected ProductReview() { }

        private ProductReview(Guid productId, Guid userId, string title, string comment, int rating)
        {
            ProductId = productId;
            UserId = userId;
            SetTitle(title);
            SetComment(comment);
            SetRating(rating);
            IsVerified = false;
            IsApproved = false;
            Deleted = false;
        }

        public static ProductReview Create(Guid productId, Guid userId, string title, string comment, int rating)
            => new(productId, userId, title, comment, rating);

        public void SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("عنوان نظر نباید خالی باشد");
            Title = title.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetComment(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("متن نظر نباید خالی باشد");
            Comment = comment.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetRating(int rating)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("امتیاز باید بین 1 تا 5 باشد");
            Rating = rating;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsVerified()
        {
            IsVerified = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAdminNotes(string? adminNotes)
        {
            AdminNotes = adminNotes?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void Approve(string? approvedBy, string? adminNotes = null)
        {
            IsApproved = true;
            ApprovedAt = DateTime.UtcNow;
            ApprovedBy = approvedBy;
            SetAdminNotes(adminNotes);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject(string? rejectedBy, string? rejectionReason = null, string? adminNotes = null)
        {
            IsApproved = false;
            ApprovedAt = null;
            ApprovedBy = null;
            RejectedAt = DateTime.UtcNow;
            RejectedBy = rejectedBy;
            RejectionReason = rejectionReason?.Trim();
            SetAdminNotes(adminNotes);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string title, string comment, int rating, string? updatedBy)
        {
            SetTitle(title);
            SetComment(comment);
            SetRating(rating);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این نظر قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
