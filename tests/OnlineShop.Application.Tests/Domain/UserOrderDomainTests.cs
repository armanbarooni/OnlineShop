using Xunit;

namespace OnlineShop.Application.Tests.Domain
{
    public class UserOrderDomainTests
    {
        [Fact]
        public void Create_ValidData_CreatesOrder()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                userId,
                orderNumber: "ORD-001",
                subTotal: 1000,
                taxAmount: 90,
                shippingAmount: 50,
                discountAmount: 100,
                totalAmount: 1040,
                currency: "IRR"
            );

            // Assert
            Assert.NotNull(order);
            Assert.Equal(userId, order.UserId);
            Assert.Equal("ORD-001", order.OrderNumber);
            Assert.Equal(1000, order.SubTotal);
            Assert.Equal(90, order.TaxAmount);
            Assert.Equal(50, order.ShippingAmount);
            Assert.Equal(100, order.DiscountAmount);
            Assert.Equal(1040, order.TotalAmount);
            Assert.Equal("IRR", order.Currency);
            Assert.Equal("Pending", order.OrderStatus);
            Assert.False(order.Deleted);
        }

        [Fact]
        public void StartProcessing_FromPending_ChangesStatusToProcessing()
        {
            // Arrange
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                Guid.NewGuid(),
                "ORD-001",
                1000,
                90,
                50,
                0,
                1140
            );

            // Act
            order.StartProcessing("admin123");

            // Assert
            Assert.Equal("Processing", order.OrderStatus);
            Assert.Equal("admin123", order.UpdatedBy);
        }

        [Fact]
        public void StartProcessing_FromNonPendingStatus_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                Guid.NewGuid(),
                "ORD-001",
                1000,
                90,
                50,
                0,
                1140
            );
            order.StartProcessing();
            // Now status is "Processing"

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                order.StartProcessing()
            );
            Assert.Contains("Pending", exception.Message);
        }

        [Fact]
        public void MarkAsShipped_FromProcessing_ChangesStatusToShipped()
        {
            // Arrange
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                Guid.NewGuid(),
                "ORD-001",
                1000,
                90,
                50,
                0,
                1140
            );
            order.StartProcessing();

            // Act
            order.MarkAsShipped("TRACK123", "admin123");

            // Assert
            Assert.Equal("Shipped", order.OrderStatus);
            Assert.Equal("TRACK123", order.TrackingNumber);
            Assert.Equal("admin123", order.UpdatedBy);
            Assert.NotNull(order.ShippedAt);
        }

        [Fact]
        public void Ship_FromInvalidStatus_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                Guid.NewGuid(),
                "ORD-001",
                1000,
                90,
                50,
                0,
                1140
            );

            // Move order through states: Pending → Processing → Shipped → Delivered
            order.StartProcessing();
            order.Ship("TRACK123");
            order.Deliver();
            
            // Act & Assert: Try to ship from Delivered (invalid)
            var exception = Assert.Throws<InvalidOperationException>(() =>
                order.Ship("TRACK456")
            );
            Assert.Contains("Processing", exception.Message);
        }

        [Fact]
        public void MarkAsDelivered_FromShipped_ChangesStatusToDelivered()
        {
            // Arrange
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                Guid.NewGuid(),
                "ORD-001",
                1000,
                90,
                50,
                0,
                1140
            );
            order.StartProcessing();
            order.MarkAsShipped("TRACK123");

            // Act
            order.MarkAsDelivered("admin123");

            // Assert
            Assert.Equal("Delivered", order.OrderStatus);
            Assert.Equal("admin123", order.UpdatedBy);
            Assert.NotNull(order.DeliveredAt);
        }

        [Fact]
        public void Deliver_FromNonShippedStatus_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                Guid.NewGuid(),
                "ORD-001",
                1000,
                90,
                50,
                0,
                1140
            );
            // Status is "Pending"

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                order.Deliver()
            );
            // Check exception message contains "Shipped" or Persian equivalent
            Assert.True(exception.Message.Contains("Shipped") || exception.Message.Contains("ارسال"));
        }

        [Fact]
        public void Cancel_FromPending_CancelsOrder()
        {
            // Arrange
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                Guid.NewGuid(),
                "ORD-001",
                1000,
                90,
                50,
                0,
                1140
            );

            // Act
            order.Cancel("Customer requested cancellation", "user123");

            // Assert
            Assert.Equal("Cancelled", order.OrderStatus);
            Assert.Equal("Customer requested cancellation", order.CancellationReason);
            Assert.Equal("user123", order.UpdatedBy);
            Assert.NotNull(order.CancelledAt);
        }

        [Fact]
        public void Cancel_FromDelivered_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                Guid.NewGuid(),
                "ORD-001",
                1000,
                90,
                50,
                0,
                1140
            );
            order.StartProcessing();
            order.Ship("TRACK123");
            order.Deliver();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                order.Cancel("Too late")
            );
            Assert.Contains("تحویل داده شده", exception.Message);
        }

        [Fact]
        public void SetTrackingNumber_ValidNumber_UpdatesTrackingNumber()
        {
            // Arrange
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                Guid.NewGuid(),
                "ORD-001",
                1000,
                90,
                50,
                0,
                1140
            );

            // Act
            order.SetTrackingNumber("TRACK123");

            // Assert
            Assert.Equal("TRACK123", order.TrackingNumber);
        }

        [Fact]
        public void SetShippingAddress_ValidGuid_UpdatesShippingAddressId()
        {
            // Arrange
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                Guid.NewGuid(),
                "ORD-001",
                1000,
                90,
                50,
                0,
                1140
            );
            var addressId = Guid.NewGuid();

            // Act
            order.SetShippingAddress(addressId);

            // Assert
            Assert.Equal(addressId, order.ShippingAddressId);
        }

        [Fact]
        public void Update_ValidData_UpdatesAllFields()
        {
            // Arrange
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                Guid.NewGuid(),
                "ORD-001",
                1000,
                90,
                50,
                0,
                1140
            );

            // Act
            order.Update(
                subTotal: 1500,
                taxAmount: 135,
                shippingAmount: 75,
                discountAmount: 50,
                totalAmount: 1660,
                notes: "Updated order",
                updatedBy: "admin123"
            );

            // Assert
            Assert.Equal(1500, order.SubTotal);
            Assert.Equal(135, order.TaxAmount);
            Assert.Equal(75, order.ShippingAmount);
            Assert.Equal(50, order.DiscountAmount);
            Assert.Equal(1660, order.TotalAmount);
            Assert.Equal("Updated order", order.Notes);
            Assert.Equal("admin123", order.UpdatedBy);
        }

        [Fact]
        public void Delete_NotDeleted_MarksAsDeleted()
        {
            // Arrange
            var order = OnlineShop.Domain.Entities.UserOrder.Create(
                Guid.NewGuid(),
                "ORD-001",
                1000,
                90,
                50,
                0,
                1140
            );

            // Act
            order.Delete("admin123");

            // Assert
            Assert.True(order.Deleted);
            Assert.Equal("admin123", order.UpdatedBy);
        }
    }
}

