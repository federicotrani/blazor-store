using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StorePro.Api.DTOs;
using StorePro.Api.Repositories.Interfaces;

namespace StorePro.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await unitOfWork.Categories.GetAllAsync();
        var result = new List<CategoryDto>(categories.Count);

        foreach (var category in categories)
        {
            var count = await unitOfWork.Categories.CountProductsAsync(category.Id);
            result.Add(new CategoryDto(category.Id, category.Name, category.Description, category.IsActive, count, category.CreatedAt));
        }

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) return NotFound();

        var count = await unitOfWork.Categories.CountProductsAsync(category.Id);
        return Ok(new CategoryDto(category.Id, category.Name, category.Description, category.IsActive, count, category.CreatedAt));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request)
    {
        if (await unitOfWork.Categories.ExistsByNameAsync(request.Name))
            return Conflict(new { message = "Ya existe una categoría con ese nombre." });

        var category = new Entities.Category
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Categories.AddAsync(category);
        await unitOfWork.SaveChangesAsync();

        var dto = new CategoryDto(category.Id, category.Name, category.Description, category.IsActive, 0, category.CreatedAt);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, UpdateCategoryRequest request)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) return NotFound();

        if (await unitOfWork.Categories.ExistsByNameAsync(request.Name, id))
            return Conflict(new { message = "Ya existe una categoría con ese nombre." });

        category.Name = request.Name.Trim();
        category.Description = request.Description;
        category.IsActive = request.IsActive;

        unitOfWork.Categories.Update(category);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) return NotFound();

        var productCount = await unitOfWork.Categories.CountProductsAsync(id);
        if (productCount > 0)
            return BadRequest(new { message = $"No se puede eliminar: la categoría tiene {productCount} producto(s) asociado(s)." });

        unitOfWork.Categories.Remove(category);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}
