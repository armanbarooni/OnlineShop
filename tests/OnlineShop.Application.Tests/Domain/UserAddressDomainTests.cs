using Xunit;

namespace OnlineShop.Application.Tests.Domain
{
    public class UserAddressDomainTests
    {
        [Fact]
        public void Create_ValidData_CreatesAddress()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var address = OnlineShop.Domain.Entities.UserAddress.Create(
                userId,
                title: "Home",
                firstName: "Ali",
                lastName: "Ahmadi",
                addressLine1: "Street 1",
                addressLine2: "Building 2",
                city: "Tehran",
                state: "Tehran",
                postalCode: "1234567890",
                country: "Iran",
                phoneNumber: "09123456789"
            );

            // Assert
            Assert.NotNull(address);
            Assert.Equal(userId, address.UserId);
            Assert.Equal("Home", address.Title);
            Assert.Equal("Ali", address.FirstName);
            Assert.Equal("Ahmadi", address.LastName);
            Assert.Equal("Street 1", address.AddressLine1);
            Assert.Equal("Building 2", address.AddressLine2);
            Assert.Equal("Tehran", address.City);
            Assert.Equal("Tehran", address.State);
            Assert.Equal("1234567890", address.PostalCode);
            Assert.Equal("Iran", address.Country);
            Assert.Equal("09123456789", address.PhoneNumber);
            Assert.False(address.IsDefault);
            Assert.False(address.Deleted);
        }

        [Fact]
        public void Create_EmptyTitle_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                OnlineShop.Domain.Entities.UserAddress.Create(
                    Guid.NewGuid(),
                    "",
                    "Ali",
                    "Ahmadi",
                    "Street",
                    null,
                    "City",
                    "State",
                    "1234567890",
                    "Country"
                )
            );
            Assert.Contains("عنوان آدرس", exception.Message);
        }

        [Fact]
        public void SetAsDefault_SetsIsDefaultToTrue()
        {
            // Arrange
            var address = OnlineShop.Domain.Entities.UserAddress.Create(
                Guid.NewGuid(),
                "Home",
                "Ali",
                "Ahmadi",
                "Street",
                null,
                "City",
                "State",
                "1234567890",
                "Country"
            );

            // Act
            address.SetAsDefault();

            // Assert
            Assert.True(address.IsDefault);
        }

        [Fact]
        public void UnsetAsDefault_SetsIsDefaultToFalse()
        {
            // Arrange
            var address = OnlineShop.Domain.Entities.UserAddress.Create(
                Guid.NewGuid(),
                "Home",
                "Ali",
                "Ahmadi",
                "Street",
                null,
                "City",
                "State",
                "1234567890",
                "Country"
            );
            address.SetAsDefault();

            // Act
            address.UnsetAsDefault();

            // Assert
            Assert.False(address.IsDefault);
        }

        [Fact]
        public void Update_ValidData_UpdatesAllFields()
        {
            // Arrange
            var address = OnlineShop.Domain.Entities.UserAddress.Create(
                Guid.NewGuid(),
                "Old Title",
                "Old First",
                "Old Last",
                "Old Street",
                null,
                "Old City",
                "Old State",
                "1234567890",
                "Old Country"
            );

            // Act
            address.Update(
                title: "New Title",
                firstName: "New First",
                lastName: "New Last",
                addressLine1: "New Street",
                addressLine2: "New Building",
                city: "New City",
                state: "New State",
                postalCode: "9876543210",
                country: "New Country",
                phoneNumber: "09987654321",
                updatedBy: "user123"
            );

            // Assert
            Assert.Equal("New Title", address.Title);
            Assert.Equal("New First", address.FirstName);
            Assert.Equal("New Last", address.LastName);
            Assert.Equal("New Street", address.AddressLine1);
            Assert.Equal("New Building", address.AddressLine2);
            Assert.Equal("New City", address.City);
            Assert.Equal("New State", address.State);
            Assert.Equal("9876543210", address.PostalCode);
            Assert.Equal("New Country", address.Country);
            Assert.Equal("09987654321", address.PhoneNumber);
            Assert.Equal("user123", address.UpdatedBy);
        }

        [Fact]
        public void Delete_NotDeleted_MarksAsDeleted()
        {
            // Arrange
            var address = OnlineShop.Domain.Entities.UserAddress.Create(
                Guid.NewGuid(),
                "Home",
                "Ali",
                "Ahmadi",
                "Street",
                null,
                "City",
                "State",
                "1234567890",
                "Country"
            );

            // Act
            address.Delete("user123");

            // Assert
            Assert.True(address.Deleted);
            Assert.Equal("user123", address.UpdatedBy);
        }

        [Fact]
        public void Delete_AlreadyDeleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var address = OnlineShop.Domain.Entities.UserAddress.Create(
                Guid.NewGuid(),
                "Home",
                "Ali",
                "Ahmadi",
                "Street",
                null,
                "City",
                "State",
                "1234567890",
                "Country"
            );
            address.Delete("user123");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                address.Delete("user123")
            );
            Assert.Contains("قبلاً حذف شده", exception.Message);
        }
    }
}

