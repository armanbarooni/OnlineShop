// TestHelpers/ProductMother.cs
using System.Reflection;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Tests.TestHelpers
{
    public static class ProductMother
    {
        public static Product CreateWithId(
            Guid id,
            string name = "Test Product",
            string description = "Test Description",
            decimal price = 100m,
            int stockQuantity = 10,
            long mahakClientId = 123,
            int mahakId = 1)
        {
            var product = Product.Create(name, description, price, stockQuantity, mahakClientId, mahakId);

            // Set ID using reflection
            var idProperty = typeof(Product).GetProperty("Id");
            idProperty?.SetValue(product, id);

            return product;
        }
    }
}
