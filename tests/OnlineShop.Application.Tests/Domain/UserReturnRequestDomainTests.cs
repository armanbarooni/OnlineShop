using Xunit;

namespace OnlineShop.Application.Tests.Domain
{
    public class UserReturnRequestDomainTests
    {
        [Fact]
        public void Create_ValidData_CreatesReturnRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();

            // Act
            var returnRequest = OnlineShop.Domain.Entities.UserReturnRequest.Create(
                userId,
                orderId,
                orderItemId,
                returnReason: "Defective product",
                description: "The product has a manufacturing defect",
                quantity: 2,
                refundAmount: 2000
            );

            // Assert
            Assert.NotNull(returnRequest);
            Assert.Equal(userId, returnRequest.UserId);
            Assert.Equal(orderId, returnRequest.OrderId);
            Assert.Equal(orderItemId, returnRequest.OrderItemId);
            Assert.Equal("Defective product", returnRequest.ReturnReason);
            Assert.Equal("The product has a manufacturing defect", returnRequest.Description);
            Assert.Equal(2, returnRequest.Quantity);
            Assert.Equal(2000, returnRequest.RefundAmount);
            Assert.Equal("Pending", returnRequest.RequestStatus);
            Assert.False(returnRequest.Deleted);
        }

        [Fact]
        public void Create_EmptyReturnReason_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                OnlineShop.Domain.Entities.UserReturnRequest.Create(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "",
                    "Description",
                    1,
                    1000
                )
            );
            Assert.Contains("دلیل مرجوعی", exception.Message);
        }

        [Fact]
        public void Create_InvalidQuantity_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                OnlineShop.Domain.Entities.UserReturnRequest.Create(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Reason",
                    "Description",
                    0,
                    1000
                )
            );
            Assert.Contains("تعداد", exception.Message);
        }

        [Fact]
        public void Create_NegativeRefundAmount_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                OnlineShop.Domain.Entities.UserReturnRequest.Create(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Reason",
                    "Description",
                    1,
                    -100
                )
            );
            Assert.Contains("مبلغ", exception.Message);
        }

        [Fact]
        public void Approve_PendingRequest_ApprovesRequest()
        {
            // Arrange
            var returnRequest = OnlineShop.Domain.Entities.UserReturnRequest.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Defective",
                "Description",
                1,
                1000
            );

            // Act
            returnRequest.Approve("admin123", "Approved for refund");

            // Assert
            Assert.Equal("Approved", returnRequest.RequestStatus);
            Assert.Equal("admin123", returnRequest.ApprovedBy);
            Assert.Equal("Approved for refund", returnRequest.AdminNotes);
            Assert.NotNull(returnRequest.ApprovedAt);
        }

        [Fact]
        public void Approve_NonPendingRequest_ThrowsInvalidOperationException()
        {
            // Arrange
            var returnRequest = OnlineShop.Domain.Entities.UserReturnRequest.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Defective",
                "Description",
                1,
                1000
            );
            returnRequest.Approve("admin123");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                returnRequest.Approve("admin456")
            );
            Assert.Contains("Pending", exception.Message);
        }

        [Fact]
        public void Reject_PendingRequest_RejectsRequest()
        {
            // Arrange
            var returnRequest = OnlineShop.Domain.Entities.UserReturnRequest.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Defective",
                "Description",
                1,
                1000
            );

            // Act
            returnRequest.Reject("admin123", "Not eligible for return", "Item is used");

            // Assert
            Assert.Equal("Rejected", returnRequest.RequestStatus);
            Assert.Equal("Not eligible for return", returnRequest.RejectionReason);
            Assert.Equal("admin123", returnRequest.RejectedBy);
            Assert.Equal("Item is used", returnRequest.AdminNotes);
            Assert.NotNull(returnRequest.RejectedAt);
        }

        [Fact]
        public void Reject_NonPendingRequest_ThrowsInvalidOperationException()
        {
            // Arrange
            var returnRequest = OnlineShop.Domain.Entities.UserReturnRequest.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Defective",
                "Description",
                1,
                1000
            );
            returnRequest.Approve("admin123");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                returnRequest.Reject("admin456", "Too late")
            );
            Assert.Contains("Pending", exception.Message);
        }

        [Fact]
        public void Update_ValidData_UpdatesRequest()
        {
            // Arrange
            var returnRequest = OnlineShop.Domain.Entities.UserReturnRequest.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Old Reason",
                "Old Description",
                1,
                1000
            );

            // Act
            returnRequest.Update(
                returnReason: "New Reason",
                description: "New Description",
                quantity: 3,
                refundAmount: 3000,
                updatedBy: "user123"
            );

            // Assert
            Assert.Equal("New Reason", returnRequest.ReturnReason);
            Assert.Equal("New Description", returnRequest.Description);
            Assert.Equal(3, returnRequest.Quantity);
            Assert.Equal(3000, returnRequest.RefundAmount);
            Assert.Equal("user123", returnRequest.UpdatedBy);
        }

        [Fact]
        public void Delete_NotDeleted_MarksAsDeleted()
        {
            // Arrange
            var returnRequest = OnlineShop.Domain.Entities.UserReturnRequest.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Defective",
                "Description",
                1,
                1000
            );

            // Act
            returnRequest.Delete("admin123");

            // Assert
            Assert.True(returnRequest.Deleted);
            Assert.Equal("admin123", returnRequest.UpdatedBy);
        }
    }
}

