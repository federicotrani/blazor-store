using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StorePro.Api.DTOs;
using StorePro.Api.Repositories.Interfaces;

namespace StorePro.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IUnitOfWork unitOfWork, IWebHostEnvironment environment) : ControllerBase
{
    private const int LowStockThreshold = 10;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await unitOfWork.Products.GetPagedAsync(search, categoryId, status, page, pageSize);
        return Ok(new PagedResult<ProductDto>(items.Select(ToDto).ToList(), total, page, pageSize));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await unitOfWork.Products.GetByIdWithCategoryAsync(id);
        if (product is null) return NotFound();

        return Ok(ToDto(product));
    }

    [HttpGet("stats")]
    [Authorize]
    public async Task<ActionResult<ProductStatsDto>> GetStats()
    {
        var total = await unitOfWork.Products.CountAsync();
        var lowStock = await unitOfWork.Products.CountAsync(maxStock: LowStockThreshold);
        var active = await unitOfWork.Products.CountAsync(status: Entities.ProductStatuses.Active);
        var categories = await unitOfWork.Categories.GetAllAsync();

        return Ok(new ProductStatsDto(total, lowStock, categories.Count(c => c.IsActive), active));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(request.CategoryId);
        if (category is null)
            return BadRequest(new { message = "La categoría indicada no existe." });

        if (!Entities.ProductStatuses.All.Contains(request.Status))
            return BadRequest(new { message = "Estado de producto inválido." });

        var product = new Entities.Product
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Status = request.Status,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Products.AddAsync(product);
        await unitOfWork.SaveChangesAsync();

        var created = await unitOfWork.Products.GetByIdWithCategoryAsync(product.Id);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ToDto(created!));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request)
    {
        var product = await unitOfWork.Products.GetByIdAsync(id);
        if (product is null) return NotFound();

        var category = await unitOfWork.Categories.GetByIdAsync(request.CategoryId);
        if (category is null)
            return BadRequest(new { message = "La categoría indicada no existe." });

        if (!Entities.ProductStatuses.All.Contains(request.Status))
            return BadRequest(new { message = "Estado de producto inválido." });

        product.Name = request.Name.Trim();
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.Status = request.Status;
        product.CategoryId = request.CategoryId;

        unitOfWork.Products.Update(product);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await unitOfWork.Products.GetByIdAsync(id);
        if (product is null) return NotFound();

        unitOfWork.Products.Remove(product);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:int}/image")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UploadImage(int id, IFormFile file)
    {
        var product = await unitOfWork.Products.GetByIdAsync(id);
        if (product is null) return NotFound();

        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No se ha enviado ninguna imagen." });

        if (file.Length > MaxImageSizeBytes)
            return BadRequest(new { message = "La imagen supera el tamaño máximo de 5 MB." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest(new { message = "Formato no permitido. Use jpg, jpeg, png, webp o gif." });

        var uploadsFolder = Path.Combine(environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
        Directory.CreateDirectory(uploadsFolder);

        // Eliminar imagen anterior si existe
        if (!string.IsNullOrEmpty(product.ImageUrl))
        {
            var oldFile = Path.Combine(uploadsFolder, Path.GetFileName(product.ImageUrl));
            if (System.IO.File.Exists(oldFile))
                System.IO.File.Delete(oldFile);
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using (var stream = System.IO.File.Create(Path.Combine(uploadsFolder, fileName)))
        {
            await file.CopyToAsync(stream);
        }

        product.ImageUrl = $"/uploads/{fileName}";
        unitOfWork.Products.Update(product);
        await unitOfWork.SaveChangesAsync();

        return Ok(new { imageUrl = product.ImageUrl });
    }

    private static ProductDto ToDto(Entities.Product product) =>
        new(product.Id, product.Name, product.Description, product.Price, product.Stock,
            product.Status, product.ImageUrl, product.CategoryId, product.Category?.Name ?? string.Empty, product.CreatedAt);
}
