using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StorePro.Api.DTOs;
using StorePro.Api.Repositories.Interfaces;
using StorePro.Api.Services;

namespace StorePro.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IUnitOfWork unitOfWork,
    IPasswordService passwordService,
    ITokenService tokenService,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await unitOfWork.Users.GetByEmailAsync(request.Email);
        if (user is null || !passwordService.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Credenciales inválidas." });

        if (user.Status == "Suspended")
            return Unauthorized(new { message = "La cuenta está suspendida. Contacte al administrador." });

        user.LastActiveAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync();

        var (token, expiresAt) = tokenService.CreateToken(user);
        logger.LogInformation("Inicio de sesión correcto para {Email}", user.Email);

        return Ok(new AuthResponse(token, expiresAt, ToDto(user)));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (await unitOfWork.Users.ExistsByEmailAsync(request.Email))
            return Conflict(new { message = "El correo electrónico ya está registrado." });

        var user = new Entities.User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = passwordService.Hash(request.Password),
            Role = Entities.UserRoles.Customer,
            Status = Entities.UserStatuses.Active,
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Users.AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        var (token, expiresAt) = tokenService.CreateToken(user);
        logger.LogInformation("Nuevo registro de usuario {Email}", user.Email);

        return Ok(new AuthResponse(token, expiresAt, ToDto(user)));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var userId = tokenService.GetUserId(User);
        if (userId is null) return Unauthorized();

        var user = await unitOfWork.Users.GetByIdAsync(userId.Value);
        if (user is null) return NotFound();

        return Ok(ToDto(user));
    }

    private static UserDto ToDto(Entities.User user) =>
        new(user.Id, user.FullName, user.Email, user.Role, user.Status, user.LastActiveAt, user.CreatedAt);
}
