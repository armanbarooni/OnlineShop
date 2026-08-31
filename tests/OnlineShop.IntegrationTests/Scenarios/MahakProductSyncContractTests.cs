using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Mahak.Models;
using OnlineShop.Infrastructure.Persistence;
using OnlineShop.Infrastructure.Services;
using OnlineShop.IntegrationTests.Infrastructure;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class MahakProductSyncContractTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public MahakProductSyncContractTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task D1_MahakSync_WhenProductDoesNotExist_ShouldCreateNewProduct()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<MahakSyncService>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var mahakId = Random.Shared.Next(100000, 999999);

            await InvokePrivateAsync(service, "ProcessProductsAsync", new List<ProductModel>
            {
                new()
                {
                    ProductId = mahakId,
                    ProductClientId = mahakId + 1,
                    ProductCode = mahakId + 2,
                    Name = "Mahak New Product",
                    Description = "Created from Mahak",
                    RowVersion = 1
                }
            }, CancellationToken.None);

            var product = await db.Products.SingleOrDefaultAsync(p => p.MahakId == mahakId);
            product.Should().NotBeNull();
            product!.Name.Should().Be("Mahak New Product");
        }

        [Fact]
        public async Task D2_MahakSync_WhenMahakDataIsOlder_ShouldNotOverwriteSiteProduct()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<MahakSyncService>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var mahakId = Random.Shared.Next(100000, 999999);
            var product = Product.Create("Local Newer Name", "Local Newer Description", 10, 1, mahakId + 1000, mahakId);
            product.SetStockQuantity(1, DateTime.UtcNow.AddMinutes(30));
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
            await db.MahakMappings.AddAsync(MahakMapping.Create("Product", product.Id, mahakId));
            await db.SaveChangesAsync();

            await InvokePrivateAsync(service, "ProcessProductsAsync", new List<ProductModel>
            {
                new()
                {
                    ProductId = mahakId,
                    ProductClientId = mahakId + 1000,
                    ProductCode = mahakId + 2000,
                    Name = "Older Mahak Name",
                    Description = "Older Mahak Description",
                    RowVersion = 1
                }
            }, CancellationToken.None);

            var reloaded = await db.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id);
            reloaded.Name.Should().Be("Local Newer Name");
            reloaded.Description.Should().Be("Local Newer Description");
        }

        [Fact]
        public async Task D3_MahakSync_WhenMahakDataIsNewer_ShouldUpdateProduct()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<MahakSyncService>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var mahakId = Random.Shared.Next(100000, 999999);
            var product = Product.Create("Old Local Name", "Old Local Description", 10, 1, mahakId + 1000, mahakId);
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
            await db.MahakMappings.AddAsync(MahakMapping.Create("Product", product.Id, mahakId));
            await db.SaveChangesAsync();

            await InvokePrivateAsync(service, "ProcessProductsAsync", new List<ProductModel>
            {
                new()
                {
                    ProductId = mahakId,
                    ProductClientId = mahakId + 1000,
                    ProductCode = mahakId + 2000,
                    Name = "New Mahak Name",
                    Description = "New Mahak Description",
                    RowVersion = 99
                }
            }, CancellationToken.None);

            var reloaded = await db.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id);
            reloaded.Name.Should().Be("New Mahak Name");
            reloaded.Description.Should().Be("New Mahak Description");
        }

        [Fact]
        public async Task D4_MahakSync_WithNewerInventory_ShouldUpdateStock()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<MahakSyncService>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var product = await CreateMappedProductAsync(db);
            var detailId = Random.Shared.Next(100000, 999999);
            await db.MahakMappings.AddAsync(MahakMapping.Create("ProductDetail", product.Id, detailId));
            await db.SaveChangesAsync();

            await InvokePrivateAsync(service, "ProcessInventoryAsync", new List<ProductDetailStoreAssetModel>
            {
                new()
                {
                    ProductDetailStoreAssetId = detailId + 1,
                    ProductDetailId = detailId,
                    StoreId = 31940,
                    Count1 = 5,
                    UpdateDate = DateTime.UtcNow.AddMinutes(5),
                    RowVersion = 2
                }
            }, new HashSet<int>(), CancellationToken.None);

            var reloaded = await db.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id);
            reloaded.StockQuantity.Should().Be(5);
        }

        [Fact]
        public async Task D5_MahakSync_WithNewerImage_ShouldUpdatePrimaryImage()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<MahakSyncService>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var product = await CreateMappedProductAsync(db);
            await db.ProductImages.AddAsync(ProductImage.Create(product.Id, "/old.jpg", isPrimary: true));
            await db.SaveChangesAsync();

            await InvokePrivateAsync(service, "ProcessImagesAsync", new List<PhotoGalleryModel>
            {
                new() { PhotoGalleryId = 10, EntityType = 102, ItemCode = product.MahakId!.Value, PictureId = 10, IsMain = true, RowVersion = 10 }
            }, new List<PictureModel>
            {
                new() { PictureId = 10, Url = "/new.jpg", RowVersion = 10 }
            }, CancellationToken.None);

            var primaryImages = await db.ProductImages.AsNoTracking()
                .Where(i => i.ProductId == product.Id && i.IsPrimary && !i.Deleted)
                .ToListAsync();

            primaryImages.Should().ContainSingle();
            primaryImages.Single().ImageUrl.Should().Be("/new.jpg");
        }

        [Fact]
        public async Task D6_MahakSync_WithNewerDescription_ShouldUpdateDescription()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<MahakSyncService>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var product = await CreateMappedProductAsync(db, description: "Old description");

            await InvokePrivateAsync(service, "ProcessProductsAsync", new List<ProductModel>
            {
                new()
                {
                    ProductId = product.MahakId!.Value,
                    ProductClientId = (int)product.MahakClientId!.Value,
                    ProductCode = product.MahakId.Value + 10,
                    Name = product.Name,
                    Description = "Updated Mahak description",
                    RowVersion = 100
                }
            }, CancellationToken.None);

            var reloaded = await db.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id);
            reloaded.Description.Should().Be("Updated Mahak description");
        }

        [Fact]
        public async Task D7_MahakSync_WithNewerPrice_ShouldUpdatePrice()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<MahakSyncService>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var product = await CreateMappedProductAsync(db, price: 100);

            await InvokePrivateAsync(service, "ProcessProductDetailsAsync", new List<ProductDetailModel>
            {
                new()
                {
                    ProductDetailId = Random.Shared.Next(100000, 999999),
                    ProductDetailClientId = Random.Shared.Next(100000, 999999),
                    ProductId = product.MahakId!.Value,
                    Price1 = 250,
                    DefaultSellPriceLevel = 1,
                    RowVersion = 100
                }
            }, CancellationToken.None);

            var reloaded = await db.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id);
            reloaded.Price.Should().Be(250);
        }

        [Fact]
        public async Task D8_MahakSync_WithNewerSize_ShouldUpdateVariantSize()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<MahakSyncService>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var product = await CreateMappedProductAsync(db);
            var detailId = Random.Shared.Next(100000, 999999);

            await InvokePrivateAsync(service, "ProcessProductDetailsAsync", new List<ProductDetailModel>
            {
                new()
                {
                    ProductDetailId = detailId,
                    ProductDetailClientId = detailId + 1,
                    ProductId = product.MahakId!.Value,
                    Barcode = $"PD-{detailId}",
                    Properties = "[{\"C\":\"3\",\"V\":\"Black\"},{\"C\":\"4\",\"V\":\"XL\"}]",
                    RowVersion = 100
                }
            }, CancellationToken.None);

            var variant = await db.ProductVariants.AsNoTracking().SingleOrDefaultAsync(v => v.ProductId == product.Id);
            variant.Should().NotBeNull();
            variant!.Size.Should().Be("XL");
        }

        [Fact]
        public async Task D9_MahakSync_WhenPictureAndGalleryArriveSeparately_ShouldAttachImage()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<MahakSyncService>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var product = await CreateMappedProductAsync(db);
            var pictureId = Random.Shared.Next(1000000, 1999999);
            var galleryId = Random.Shared.Next(2000000, 2999999);

            await InvokePrivateAsync(service, "ProcessImagesAsync",
                new List<PhotoGalleryModel>(),
                new List<PictureModel>
                {
                    new() { PictureId = pictureId, Url = $"/pic-{pictureId}-new.jpg", RowVersion = 10 }
                },
                CancellationToken.None);

            await InvokePrivateAsync(service, "ProcessImagesAsync",
                new List<PhotoGalleryModel>
                {
                    new()
                    {
                        PhotoGalleryId = galleryId,
                        EntityType = 102,
                        ItemCode = product.MahakId!.Value,
                        PictureId = pictureId,
                        IsMain = true,
                        RowVersion = 11
                    }
                },
                new List<PictureModel>(),
                CancellationToken.None);

            var image = await db.ProductImages.AsNoTracking()
                .SingleOrDefaultAsync(item => item.ProductId == product.Id && !item.Deleted);
            image.Should().NotBeNull();
            image!.MahakId.Should().Be(pictureId);
            image.ImageUrl.Should().Be($"/pic-{pictureId}-new.jpg");
            image.IsPrimary.Should().BeTrue();
        }

        [Fact]
        public async Task D10_MahakSync_WhenOldGalleryIsDeleted_ShouldReplaceOldMahakImage()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<MahakSyncService>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var product = await CreateMappedProductAsync(db);
            var oldPictureId = Random.Shared.Next(3000000, 3999999);
            var newPictureId = Random.Shared.Next(4000000, 4999999);
            var oldGalleryId = Random.Shared.Next(5000000, 5999999);
            var newGalleryId = Random.Shared.Next(6000000, 6999999);

            await InvokePrivateAsync(service, "ProcessImagesAsync",
                new List<PhotoGalleryModel>
                {
                    new() { PhotoGalleryId = oldGalleryId, EntityType = 102, ItemCode = product.MahakId!.Value, PictureId = oldPictureId, IsMain = true, RowVersion = 10 }
                },
                new List<PictureModel>
                {
                    new() { PictureId = oldPictureId, Url = $"/pic-{oldPictureId}-old.jpg", RowVersion = 10 }
                },
                CancellationToken.None);

            await InvokePrivateAsync(service, "ProcessImagesAsync",
                new List<PhotoGalleryModel>
                {
                    new() { PhotoGalleryId = oldGalleryId, EntityType = 102, ItemCode = product.MahakId!.Value, PictureId = oldPictureId, IsMain = true, Deleted = true, RowVersion = 20 },
                    new() { PhotoGalleryId = newGalleryId, EntityType = 102, ItemCode = product.MahakId.Value, PictureId = newPictureId, IsMain = true, RowVersion = 21 }
                },
                new List<PictureModel>
                {
                    new() { PictureId = newPictureId, Url = $"/pic-{newPictureId}-new.jpg", RowVersion = 20 }
                },
                CancellationToken.None);

            var activeImages = await db.ProductImages.AsNoTracking()
                .Where(item => item.ProductId == product.Id && !item.Deleted)
                .ToListAsync();
            activeImages.Should().ContainSingle();
            activeImages.Single().MahakId.Should().Be(newPictureId);
            activeImages.Single().ImageUrl.Should().Be($"/pic-{newPictureId}-new.jpg");
        }

        [Fact]
        public async Task D11_MahakSync_WhenOlderProductPayloadArrivesLater_ShouldKeepNewestName()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<MahakSyncService>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var product = await CreateMappedProductAsync(db, name: "Initial name");

            await InvokePrivateAsync(service, "ProcessProductsAsync", new List<ProductModel>
            {
                new()
                {
                    ProductId = product.MahakId!.Value,
                    ProductClientId = (int)product.MahakClientId!.Value,
                    ProductCode = product.MahakId.Value + 10,
                    Name = "Newest Mahak name",
                    RowVersion = 200
                }
            }, CancellationToken.None);

            await InvokePrivateAsync(service, "ProcessProductsAsync", new List<ProductModel>
            {
                new()
                {
                    ProductId = product.MahakId.Value,
                    ProductClientId = (int)product.MahakClientId.Value,
                    ProductCode = product.MahakId.Value + 10,
                    Name = "Stale deleted-product name",
                    RowVersion = 100
                }
            }, CancellationToken.None);

            var reloaded = await db.Products.AsNoTracking().SingleAsync(item => item.Id == product.Id);
            reloaded.Name.Should().Be("Newest Mahak name");
        }

        private static async Task InvokePrivateAsync(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Should().NotBeNull($"private method {methodName} should exist");

            var result = method!.Invoke(target, args);
            if (result is Task task)
            {
                await task;
            }
        }

        private static async Task<Product> CreateMappedProductAsync(
            ApplicationDbContext db,
            string name = "Local Mahak Product",
            string description = "Local description",
            decimal price = 100,
            int stock = 1)
        {
            var mahakId = Random.Shared.Next(100000, 999999);
            var product = Product.Create(name, description, price, stock, mahakId + 1000, mahakId);
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
            await db.MahakMappings.AddAsync(MahakMapping.Create("Product", product.Id, mahakId));
            await db.SaveChangesAsync();
            return product;
        }
    }
}
