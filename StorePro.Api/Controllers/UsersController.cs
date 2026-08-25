using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StorePro.Api.DTOs;
using StorePro.Api.Repositories.Interfaces;
using StorePro.Api.Services;

namespace StorePro.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IUnitOfWork unitOfWork, IPasswordService passwordService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PagedResult<UserDetailDto>>> GetUsers(
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await unitOfWork.Users.GetPagedAsync(search, role, status, page, pageSize);
        return Ok(new PagedResult<UserDetailDto>(items.Select(ToDto).ToList(), total, page, pageSize));
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<UserStatsDto>> GetStats()
    {
        var total = await unitOfWork.Users.CountAsync();
        var active = await unitOfWork.Users.CountAsync(status: Entities.UserStatuses.Active);
        var admins = await unitOfWork.Users.CountAsync(role: Entities.UserRoles.Admin);
        var pending = Math.Max(0, total - active - (total - active) / 2);

        return Ok(new UserStatsDto(total, active, admins, pending));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDetailDto>> GetUser(int id)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id);
        if (user is null) return NotFound();

        return Ok(ToDto(user));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDetailDto>> Create(CreateUserRequest request)
    {
        if (!Entities.UserRoles.All.Contains(request.Role))
            return BadRequest(new { message = "Rol inválido." });

        if (await unitOfWork.Users.ExistsByEmailAsync(request.Email))
            return Conflict(new { message = "El correo electrónico ya está registrado." });

        var user = new Entities.User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = passwordService.Hash(request.Password),
            Role = request.Role,
            Status = Entities.UserStatuses.All.Contains(request.Status) ? request.Status : Entities.UserStatuses.Active,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Users.AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ToDto(user));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateUserRequest request)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id);
        if (user is null) return NotFound();

        if (!Entities.UserRoles.All.Contains(request.Role))
            return BadRequest(new { message = "Rol inválido." });

        if (await unitOfWork.Users.ExistsByEmailAsync(request.Email, id))
            return Conflict(new { message = "El correo electrónico ya está registrado." });

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim().ToLower();
        user.Role = request.Role;
        user.Status = Entities.UserStatuses.All.Contains(request.Status) ? request.Status : user.Status;

        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = passwordService.Hash(request.Password);

        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateUserStatusRequest request)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id);
        if (user is null) return NotFound();

        if (!Entities.UserStatuses.All.Contains(request.Status))
            return BadRequest(new { message = "Estado inválido." });

        user.Status = request.Status;
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id);
        if (user is null) return NotFound();

        unitOfWork.Users.Remove(user);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private static UserDetailDto ToDto(Entities.User user) =>
        new(user.Id, user.FullName, user.Email, user.Role, user.Status, user.LastActiveAt, user.CreatedAt);
}
