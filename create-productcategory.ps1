# تعیین مسیر ریشه پروژه
$projectRoot = $PSScriptRoot

# تعریف مسیرها
$paths = @{
    Entity = Join-Path $projectRoot "src\Domain\Entites"
    Repository = Join-Path $projectRoot "src\Infrastructure\Persistence\Repositories"
    RepositoryInterface = Join-Path $projectRoot "src\Application\Contracts\Persistence\InterFaces\Repositories"
    DTOs = Join-Path $projectRoot "src\Application\DTOs\ProductCategory"
    Commands = Join-Path $projectRoot "src\Application\Features\ProductCategory\Command"
    Queries = Join-Path $projectRoot "src\Application\Features\ProductCategory\Queries"
    Configuration = Join-Path $projectRoot "src\Infrastructure\DbConfigurations"
    Controller = Join-Path $projectRoot "src\Presentation\Controllers"
}

# ایجاد پوشه‌ها
foreach ($path in $paths.Values) {
    if (-not (Test-Path $path)) {
        New-Item -Path $path -ItemType Directory -Force | Out-Null
    }
}

# ===== 1. Entity =====
$entityContent = @'
using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class ProductCategory : BaseEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        protected ProductCategory() { }

        private ProductCategory(string name, string description, long mahakClientId, int mahakId)
        {
            SetName(name);
            SetDescription(description);
            MahakClientId = mahakClientId;
            MahakId = mahakId;
            Deleted = false;
        }

        public static ProductCategory Create(string name, string description, long mahakClientId, int mahakId)
            => new(name, description, mahakClientId, mahakId);

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be empty");
            Name = name.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDescription(string description)
        {
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string description, int? updatedBy)
        {
            SetName(name);
            SetDescription(description);
            UpdatedBy = updatedBy ?? 1;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(int? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("This category is already deleted.");
            Deleted = true;
            UpdatedBy = updatedBy ?? 1;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
'@
Set-Content -Path (Join-Path $paths.Entity "ProductCategory.cs") -Value $entityContent -Encoding UTF8

# ===== 2. Repository Interface =====
$repoInterfaceContent = @'
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface IProductCategoryRepository
    {
        Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
        Task UpdateAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
        Task DeleteAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
'@
Set-Content -Path (Join-Path $paths.RepositoryInterface "IProductCategoryRepository.cs") -Value $repoInterfaceContent -Encoding UTF8

# ===== 3. Repository Implementation =====
$repoImplContent = @'
using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ProductCategories
                .FirstOrDefaultAsync(pc => pc.Id == id && !pc.Deleted, cancellationToken);
        }

        public async Task<IEnumerable<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ProductCategories
                .Where(pc => !pc.Deleted)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
        {
            await _context.ProductCategories.AddAsync(productCategory, cancellationToken);
        }

        public Task UpdateAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
        {
            _context.ProductCategories.Update(productCategory);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
        {
            _context.ProductCategories.Remove(productCategory);
            return Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
'@
Set-Content -Path (Join-Path $paths.Repository "ProductCategoryRepository.cs") -Value $repoImplContent -Encoding UTF8

# ===== 4. DTOs =====
$createDtoContent = @'
namespace OnlineShop.Application.DTOs.ProductCategory
{
    public class CreateProductCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public long MahakClientId { get; set; }
        public int MahakId { get; set; }
    }
}
'@
Set-Content -Path (Join-Path $paths.DTOs "CreateProductCategoryDto.cs") -Value $createDtoContent -Encoding UTF8

$updateDtoContent = @'
namespace OnlineShop.Application.DTOs.ProductCategory
{
    public class UpdateProductCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
'@
Set-Content -Path (Join-Path $paths.DTOs "UpdateProductCategoryDto.cs") -Value $updateDtoContent -Encoding UTF8

$productCategoryDtoContent = @'
namespace OnlineShop.Application.DTOs.ProductCategory
{
    public class ProductCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? MahakId { get; set; }
        public long? MahakClientId { get; set; }
    }
}
'@
Set-Content -Path (Join-Path $paths.DTOs "ProductCategoryDto.cs") -Value $productCategoryDtoContent -Encoding UTF8

$detailsDtoContent = @'
namespace OnlineShop.Application.DTOs.ProductCategory
{
    public class ProductCategoryDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? MahakId { get; set; }
        public long? MahakClientId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
'@
Set-Content -Path (Join-Path $paths.DTOs "ProductCategoryDetailsDto.cs") -Value $detailsDtoContent -Encoding UTF8

# ===== 5. Commands =====
# Create
New-Item -Path (Join-Path $paths.Commands "Create") -ItemType Directory -Force | Out-Null
$createCommandContent = @'
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Command.Create
{
    public record CreateProductCategoryCommand(CreateProductCategoryDto Dto);
}
'@
Set-Content -Path (Join-Path $paths.Commands "Create\CreateProductCategoryCommand.cs") -Value $createCommandContent -Encoding UTF8

$createHandlerContent = @'
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Features.ProductCategory.Command.Create
{
    public class CreateProductCategoryCommandHandler
    {
        private readonly IProductCategoryRepository _repository;

        public CreateProductCategoryCommandHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ProductCategoryDto>> Handle(CreateProductCategoryCommand command, CancellationToken cancellationToken)
        {
            var productCategory = Domain.Entities.ProductCategory.Create(
                command.Dto.Name,
                command.Dto.Description,
                command.Dto.MahakClientId,
                command.Dto.MahakId
            );

            await _repository.AddAsync(productCategory, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            var dto = new ProductCategoryDto
            {
                Id = productCategory.Id,
                Name = productCategory.Name,
                Description = productCategory.Description,
                MahakId = productCategory.MahakId,
                MahakClientId = productCategory.MahakClientId
            };

            return Result<ProductCategoryDto>.Success(dto);
        }
    }
}
'@
Set-Content -Path (Join-Path $paths.Commands "Create\CreateProductCategoryCommandHandler.cs") -Value $createHandlerContent -Encoding UTF8

# Update
New-Item -Path (Join-Path $paths.Commands "Update") -ItemType Directory -Force | Out-Null
$updateCommandContent = @'
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Command.Update
{
    public record UpdateProductCategoryCommand(Guid Id, UpdateProductCategoryDto Dto);
}
'@
Set-Content -Path (Join-Path $paths.Commands "Update\UpdateProductCategoryCommand.cs") -Value $updateCommandContent -Encoding UTF8

$updateHandlerContent = @'
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.Exceptions;

namespace OnlineShop.Application.Features.ProductCategory.Command.Update
{
    public class UpdateProductCategoryCommandHandler
    {
        private readonly IProductCategoryRepository _repository;

        public UpdateProductCategoryCommandHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ProductCategoryDto>> Handle(UpdateProductCategoryCommand command, CancellationToken cancellationToken)
        {
            var productCategory = await _repository.GetByIdAsync(command.Id, cancellationToken);
            if (productCategory == null)
                throw new NotFoundException($"ProductCategory with ID {command.Id} not found.");

            productCategory.Update(command.Dto.Name, command.Dto.Description, null);
            await _repository.UpdateAsync(productCategory, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            var dto = new ProductCategoryDto
            {
                Id = productCategory.Id,
                Name = productCategory.Name,
                Description = productCategory.Description,
                MahakId = productCategory.MahakId,
                MahakClientId = productCategory.MahakClientId
            };

            return Result<ProductCategoryDto>.Success(dto);
        }
    }
}
'@
Set-Content -Path (Join-Path $paths.Commands "Update\UpdateProductCategoryCommandHandler.cs") -Value $updateHandlerContent -Encoding UTF8

# Delete
New-Item -Path (Join-Path $paths.Commands "Delete") -ItemType Directory -Force | Out-Null
$deleteCommandContent = @'
namespace OnlineShop.Application.Features.ProductCategory.Command.Delete
{
    public record DeleteProductCategoryCommand(Guid Id);
}
'@
Set-Content -Path (Join-Path $paths.Commands "Delete\DeleteProductCategoryCommand.cs") -Value $deleteCommandContent -Encoding UTF8

$deleteHandlerContent = @'
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.Exceptions;

namespace OnlineShop.Application.Features.ProductCategory.Command.Delete
{
    public class DeleteProductCategoryCommandHandler
    {
        private readonly IProductCategoryRepository _repository;

        public DeleteProductCategoryCommandHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteProductCategoryCommand command, CancellationToken cancellationToken)
        {
            var productCategory = await _repository.GetByIdAsync(command.Id, cancellationToken);
            if (productCategory == null)
                throw new NotFoundException($"ProductCategory with ID {command.Id} not found.");

            productCategory.Delete(null);
            await _repository.UpdateAsync(productCategory, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
'@
Set-Content -Path (Join-Path $paths.Commands "Delete\DeleteProductCategoryCommandHandler.cs") -Value $deleteHandlerContent -Encoding UTF8

# ===== 6. Queries =====
# GetAll
New-Item -Path (Join-Path $paths.Queries "GetAll") -ItemType Directory -Force | Out-Null
$getAllQueryContent = @'
namespace OnlineShop.Application.Features.ProductCategory.Queries.GetAll
{
    public record GetAllProductCategoriesQuery();
}
'@
Set-Content -Path (Join-Path $paths.Queries "GetAll\GetAllProductCategoriesQuery.cs") -Value $getAllQueryContent -Encoding UTF8

$getAllHandlerContent = @'
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Queries.GetAll
{
    public class GetAllProductCategoriesQueryHandler
    {
        private readonly IProductCategoryRepository _repository;

        public GetAllProductCategoriesQueryHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IEnumerable<ProductCategoryDto>>> Handle(GetAllProductCategoriesQuery query, CancellationToken cancellationToken)
        {
            var productCategories = await _repository.GetAllAsync(cancellationToken);
            var dtos = productCategories.Select(pc => new ProductCategoryDto
            {
                Id = pc.Id,
                Name = pc.Name,
                Description = pc.Description,
                MahakId = pc.MahakId,
                MahakClientId = pc.MahakClientId
            });

            return Result<IEnumerable<ProductCategoryDto>>.Success(dtos);
        }
    }
}
'@
Set-Content -Path (Join-Path $paths.Queries "GetAll\GetAllProductCategoriesQueryHandler.cs") -Value $getAllHandlerContent -Encoding UTF8

# GetById
New-Item -Path (Join-Path $paths.Queries "GetById") -ItemType Directory -Force | Out-Null
$getByIdQueryContent = @'
namespace OnlineShop.Application.Features.ProductCategory.Queries.GetById
{
    public record GetProductCategoryByIdQuery(Guid Id);
}
'@
Set-Content -Path (Join-Path $paths.Queries "GetById\GetProductCategoryByIdQuery.cs") -Value $getByIdQueryContent -Encoding UTF8

$getByIdHandlerContent = @'
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.Exceptions;

namespace OnlineShop.Application.Features.ProductCategory.Queries.GetById
{
    public class GetProductCategoryByIdQueryHandler
    {
        private readonly IProductCategoryRepository _repository;

        public GetProductCategoryByIdQueryHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ProductCategoryDetailsDto>> Handle(GetProductCategoryByIdQuery query, CancellationToken cancellationToken)
        {
            var productCategory = await _repository.GetByIdAsync(query.Id, cancellationToken);
            if (productCategory == null)
                throw new NotFoundException($"ProductCategory with ID {query.Id} not found.");

            var dto = new ProductCategoryDetailsDto
            {
                Id = productCategory.Id,
                Name = productCategory.Name,
                Description = productCategory.Description,
                MahakId = productCategory.MahakId,
                MahakClientId = productCategory.MahakClientId,
                CreatedAt = productCategory.CreatedAt,
                UpdatedAt = productCategory.UpdatedAt
            };

            return Result<ProductCategoryDetailsDto>.Success(dto);
        }
    }
}
'@
Set-Content -Path (Join-Path $paths.Queries "GetById\GetProductCategoryByIdQueryHandler.cs") -Value $getByIdHandlerContent -Encoding UTF8

# ===== 7. EF Configuration =====
$configurationContent = @'
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.DbConfigurations
{
    public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.HasKey(pc => pc.Id);
            builder.Property(pc => pc.Id).ValueGeneratedOnAdd();

            builder.Property(pc => pc.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(pc => pc.Description)
                .HasMaxLength(500);

            builder.Property(pc => pc.MahakId);
            builder.Property(pc => pc.MahakClientId);
            builder.Property(pc => pc.RowVersion).IsConcurrencyToken();
            builder.Property(pc => pc.Deleted).HasDefaultValue(false);
            builder.Property(pc => pc.CreatedAt).IsRequired();
            builder.Property(pc => pc.UpdatedAt);

            builder.HasQueryFilter(pc => !pc.Deleted);
        }
    }
}
'@
Set-Content -Path (Join-Path $paths.Configuration "ProductCategoryConfiguration.cs") -Value $configurationContent -Encoding UTF8

# ===== 8. Controller =====
$controllerContent = @'
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.Features.ProductCategory.Command.Create;
using OnlineShop.Application.Features.ProductCategory.Command.Delete;
using OnlineShop.Application.Features.ProductCategory.Command.Update;
using OnlineShop.Application.Features.ProductCategory.Queries.GetAll;
using OnlineShop.Application.Features.ProductCategory.Queries.GetById;

namespace OnlineShop.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductCategoryController : ControllerBase
    {
        private readonly GetAllProductCategoriesQueryHandler _getAllHandler;
        private readonly GetProductCategoryByIdQueryHandler _getByIdHandler;
        private readonly CreateProductCategoryCommandHandler _createHandler;
        private readonly UpdateProductCategoryCommandHandler _updateHandler;
        private readonly DeleteProductCategoryCommandHandler _deleteHandler;

        public ProductCategoryController(
            GetAllProductCategoriesQueryHandler getAllHandler,
            GetProductCategoryByIdQueryHandler getByIdHandler,
            CreateProductCategoryCommandHandler createHandler,
            UpdateProductCategoryCommandHandler updateHandler,
            DeleteProductCategoryCommandHandler deleteHandler)
        {
            _getAllHandler = getAllHandler;
            _getByIdHandler = getByIdHandler;
            _createHandler = createHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _getAllHandler.Handle(new GetAllProductCategoriesQuery(), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _getByIdHandler.Handle(new GetProductCategoryByIdQuery(id), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductCategoryDto dto, CancellationToken cancellationToken)
        {
            var result = await _createHandler.Handle(new CreateProductCategoryCommand(dto), cancellationToken);
            return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value) : BadRequest(result.Error);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCategoryDto dto, CancellationToken cancellationToken)
        {
            var result = await _updateHandler.Handle(new UpdateProductCategoryCommand(id, dto), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _deleteHandler.Handle(new DeleteProductCategoryCommand(id), cancellationToken);
            return result.IsSuccess ? NoContent() : NotFound(result.Error);
        }
    }
}
'@
Set-Content -Path (Join-Path $paths.Controller "ProductCategoryController.cs") -Value $controllerContent -Encoding UTF8

Write-Host "SUCCESS: All 19 files created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Created files:" -ForegroundColor Cyan
Write-Host "  1. Entity: ProductCategory.cs" -ForegroundColor Yellow
Write-Host "  2. Repository Interface: IProductCategoryRepository.cs" -ForegroundColor Yellow
Write-Host "  3. Repository: ProductCategoryRepository.cs" -ForegroundColor Yellow
Write-Host "  4. DTOs: 4 files (Create, Update, ProductCategory, Details)" -ForegroundColor Yellow
Write-Host "  5. Commands: 6 files (Create, Update, Delete + Handlers)" -ForegroundColor Yellow
Write-Host "  6. Queries: 4 files (GetAll, GetById + Handlers)" -ForegroundColor Yellow
Write-Host "  7. Configuration: ProductCategoryConfiguration.cs" -ForegroundColor Yellow
Write-Host "  8. Controller: ProductCategoryController.cs" -ForegroundColor Yellow
