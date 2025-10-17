using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Application.DTOs.Brand;

namespace OnlineShop.IntegrationTests.Helpers
{
    public static class TestDataHelper
    {
        public static RegisterDto CreateValidRegisterDto()
        {
            return new RegisterDto
            {
                Email = $"user{Guid.NewGuid().ToString().Substring(0, 8)}@test.com",
                Password = "Test@123456",
                ConfirmPassword = "Test@123456",
                FirstName = "Test",
                LastName = "User"
            };
        }

        public static CreateProductCategoryDto CreateValidCategoryDto(string? name = null)
        {
            return new CreateProductCategoryDto
            {
                Name = name ?? $"Category{Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = "Test Category Description"
            };
        }

        public static CreateUnitDto CreateValidUnitDto(string? name = null)
        {
            return new CreateUnitDto
            {
                Name = name ?? $"Unit{Guid.NewGuid().ToString().Substring(0, 8)}",
                Comment = "Test Unit"
            };
        }

        public static CreateBrandDto CreateValidBrandDto(string? name = null)
        {
            return new CreateBrandDto
            {
                Name = name ?? $"Brand{Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = "Test Brand Description"
            };
        }

        public static CreateProductDto CreateValidProductDto(decimal price = 100000, int stock = 50)
        {
            return new CreateProductDto
            {
                Name = $"Product{Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = "Test Product Description",
                Price = price,
                StockQuantity = stock
            };
        }
    }
}

