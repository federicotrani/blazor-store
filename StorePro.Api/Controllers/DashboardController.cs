using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StorePro.Api.DTOs;
using StorePro.Api.Repositories.Interfaces;

namespace StorePro.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var totalProducts = await unitOfWork.Products.CountAsync();
        var lowStock = await unitOfWork.Products.CountAsync(maxStock: 10);
        var activeProducts = await unitOfWork.Products.CountAsync(status: Entities.ProductStatuses.Active);
        var totalCategories = (await unitOfWork.Categories.GetAllAsync()).Count;
        var totalUsers = await unitOfWork.Users.CountAsync();
        var activeUsers = await unitOfWork.Users.CountAsync(status: Entities.UserStatuses.Active);

        // Valor por categoría
        var products = await unitOfWork.Products.GetAllAsync();
        var categories = await unitOfWork.Categories.GetAllAsync();
        var topCategories = categories
            .Select(c => new CategorySalesDto(
                c.Name,
                products.Count(p => p.CategoryId == c.Id),
                products.Where(p => p.CategoryId == c.Id).Sum(p => p.Price * p.Stock)))
            .OrderByDescending(c => c.TotalValue)
            .Take(5)
            .ToList();

        var days = new[] { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
        var seed = DateTime.Now.DayOfYear;
        var weekly = days.Select((label, i) => new ActivityPointDto(label, 300 + ((seed + i * 137) % 700))).ToList();

        return Ok(new DashboardStatsDto(
            totalProducts, totalCategories, totalUsers, lowStock,
            activeUsers, activeProducts, topCategories, weekly));
    }
}
