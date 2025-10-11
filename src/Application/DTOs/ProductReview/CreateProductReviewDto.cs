namespace OnlineShop.Application.DTOs.ProductReview
{
    public class CreateProductReviewDto
    {
        public Guid ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
    }

    public class UpdateProductReviewDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
    }

    public class ProductReviewDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsVerified { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ApproveProductReviewDto
    {
        public Guid Id { get; set; }
        public string? AdminNotes { get; set; }
    }

    public class RejectProductReviewDto
    {
        public Guid Id { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
    }
}
