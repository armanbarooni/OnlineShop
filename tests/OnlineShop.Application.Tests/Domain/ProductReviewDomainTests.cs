using Xunit;

namespace OnlineShop.Application.Tests.Domain
{
    public class ProductReviewDomainTests
    {
        [Fact]
        public void Create_ValidData_CreatesReview()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var review = OnlineShop.Domain.Entities.ProductReview.Create(
                productId,
                userId,
                title: "Great Product",
                comment: "This is an excellent product!",
                rating: 5
            );

            // Assert
            Assert.NotNull(review);
            Assert.Equal(productId, review.ProductId);
            Assert.Equal(userId, review.UserId);
            Assert.Equal("Great Product", review.Title);
            Assert.Equal("This is an excellent product!", review.Comment);
            Assert.Equal(5, review.Rating);
            Assert.False(review.IsApproved);
            Assert.False(review.IsVerified);
            Assert.False(review.Deleted);
        }

        [Fact]
        public void Create_EmptyTitle_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                OnlineShop.Domain.Entities.ProductReview.Create(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "",
                    "Comment",
                    5
                )
            );
            Assert.Contains("عنوان نظر", exception.Message);
        }

        [Fact]
        public void Create_InvalidRating_ThrowsArgumentException()
        {
            // Act & Assert - Rating less than 1
            var exception1 = Assert.Throws<ArgumentException>(() =>
                OnlineShop.Domain.Entities.ProductReview.Create(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Title",
                    "Comment",
                    0
                )
            );
            Assert.Contains("امتیاز", exception1.Message);

            // Rating greater than 5
            var exception2 = Assert.Throws<ArgumentException>(() =>
                OnlineShop.Domain.Entities.ProductReview.Create(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Title",
                    "Comment",
                    6
                )
            );
            Assert.Contains("امتیاز", exception2.Message);
        }

        [Fact]
        public void Approve_NotApprovedReview_ApprovesReview()
        {
            // Arrange
            var review = OnlineShop.Domain.Entities.ProductReview.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Title",
                "Comment",
                5
            );

            // Act
            review.Approve("admin123", "Good review");

            // Assert
            Assert.True(review.IsApproved);
            Assert.Equal("admin123", review.ApprovedBy);
            Assert.Equal("Good review", review.AdminNotes);
            Assert.NotNull(review.ApprovedAt);
        }

        [Fact]
        public void Reject_ApprovedReview_RejectsReview()
        {
            // Arrange
            var review = OnlineShop.Domain.Entities.ProductReview.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Title",
                "Comment",
                5
            );
            review.Approve("admin123");

            // Act
            review.Reject("admin456", "Inappropriate content", "Not suitable");

            // Assert
            Assert.False(review.IsApproved);
            Assert.Equal("Inappropriate content", review.RejectionReason);
            Assert.Equal("admin456", review.RejectedBy);
            Assert.Equal("Not suitable", review.AdminNotes);
            Assert.NotNull(review.RejectedAt);
        }

        [Fact]
        public void MarkAsVerified_SetsIsVerifiedToTrue()
        {
            // Arrange
            var review = OnlineShop.Domain.Entities.ProductReview.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Title",
                "Comment",
                5
            );

            // Act
            review.MarkAsVerified();

            // Assert
            Assert.True(review.IsVerified);
        }

        [Fact]
        public void Update_ValidData_UpdatesReview()
        {
            // Arrange
            var review = OnlineShop.Domain.Entities.ProductReview.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Old Title",
                "Old Comment",
                3
            );

            // Act
            review.Update(
                title: "New Title",
                comment: "New Comment",
                rating: 5,
                updatedBy: "user123"
            );

            // Assert
            Assert.Equal("New Title", review.Title);
            Assert.Equal("New Comment", review.Comment);
            Assert.Equal(5, review.Rating);
            Assert.Equal("user123", review.UpdatedBy);
        }

        [Fact]
        public void Delete_NotDeleted_MarksAsDeleted()
        {
            // Arrange
            var review = OnlineShop.Domain.Entities.ProductReview.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Title",
                "Comment",
                5
            );

            // Act
            review.Delete("admin123");

            // Assert
            Assert.True(review.Deleted);
            Assert.Equal("admin123", review.UpdatedBy);
        }
    }
}

