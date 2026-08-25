using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StorePro.Api.Data;
using StorePro.Api.Repositories;
using StorePro.Api.Repositories.Interfaces;
using StorePro.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// OpenAPI (documento) + Swagger UI para consultar los endpoints
builder.Services.AddOpenApi();

// Base de datos SQL Server Express (autenticación de Windows)
builder.Services.AddDbContext<StoreProDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorios y unidad de trabajo
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Servicios
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<DbSeeder>();

// Autenticación JWT
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// CORS para el frontend Blazor WebAssembly (cualquier puerto localhost en desarrollo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .SetIsOriginAllowed(origin =>
            origin.StartsWith("http://localhost:", StringComparison.OrdinalIgnoreCase) ||
            origin.StartsWith("https://localhost:", StringComparison.OrdinalIgnoreCase))
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "StorePro API v1");
        options.DocumentTitle = "StorePro API";
    });

    // Aplicar migraciones y datos semilla en desarrollo
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<StoreProDbContext>();
    await context.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
