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
