using Microsoft.EntityFrameworkCore;
using StorePro.Api.Services;

namespace StorePro.Api.Data;

public class DbSeeder(StoreProDbContext context, IPasswordService passwordService, ILogger<DbSeeder> logger)
{
    public async Task SeedAsync()
    {
        if (await context.Users.AnyAsync())
            return;

        logger.LogInformation("Base de datos vacía. Ejecutando datos iniciales...");

        var admin = new Entities.User
        {
            FullName = "Admin StorePro",
            Email = "admin@storepro.dev",
            PasswordHash = passwordService.Hash("Admin123$"),
            Role = Entities.UserRoles.Admin,
            Status = Entities.UserStatuses.Active,
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-90)
        };

        var users = new List<Entities.User>
        {
            admin,
            new()
            {
                FullName = "Sarah Jenkins",
                Email = "sarah.j@storepro.io",
                PasswordHash = passwordService.Hash("Admin123$"),
                Role = Entities.UserRoles.Admin,
                Status = Entities.UserStatuses.Active,
                LastActiveAt = DateTime.UtcNow.AddMinutes(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new()
            {
                FullName = "Michael Ross",
                Email = "m.ross@logistics.co",
                PasswordHash = passwordService.Hash("Manager123$"),
                Role = Entities.UserRoles.Manager,
                Status = Entities.UserStatuses.Active,
                LastActiveAt = DateTime.UtcNow.AddHours(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-45)
            },
            new()
            {
                FullName = "David Chen",
                Email = "d.chen@external.dev",
                PasswordHash = passwordService.Hash("Customer123$"),
                Role = Entities.UserRoles.Customer,
                Status = Entities.UserStatuses.Suspended,
                LastActiveAt = new DateTime(2023, 10, 12, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow.AddDays(-120)
            },
            new()
            {
                FullName = "Lucía Fernández",
                Email = "lucia.f@example.com",
                PasswordHash = passwordService.Hash("Customer123$"),
                Role = Entities.UserRoles.Customer,
                Status = Entities.UserStatuses.Active,
                LastActiveAt = DateTime.UtcNow.AddDays(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            }
        };

        var categories = new List<Entities.Category>
        {
            new() { Name = "Electrónica", Description = "Dispositivos y gadgets electrónicos", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-80) },
            new() { Name = "Accesorios", Description = "Accesorios para computadora y móvil", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-75) },
            new() { Name = "Ropa", Description = "Prendas y vestimenta urbana", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-70) },
            new() { Name = "Hogar", Description = "Artículos para el hogar y oficina", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-65) }
        };

        var products = new List<Entities.Product>
        {
            new() { Name = "Auriculares Proxima Over-Ear ANC", Description = "Auriculares con cancelación activa de ruido y 40 horas de autonomía.", Price = 299.00m, Stock = 35, Status = Entities.ProductStatuses.Active, Category = categories[0], CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new() { Name = "Teclado Mecánico K8", Description = "Teclado mecánico compacto con switches rojos retroiluminados.", Price = 149.50m, Stock = 8, Status = Entities.ProductStatuses.Draft, Category = categories[1], CreatedAt = DateTime.UtcNow.AddDays(-28) },
            new() { Name = "Mochila Urbana Commuter", Description = "Mochila impermeable para laptop de 15 pulgadas con puerto USB.", Price = 89.99m, Stock = 0, Status = Entities.ProductStatuses.OutOfStock, Category = categories[2], CreatedAt = DateTime.UtcNow.AddDays(-25) },
            new() { Name = "Smartwatch Quantum", Description = "Reloj inteligente con GPS, oxímetro y pantalla AMOLED.", Price = 349.00m, Stock = 22, Status = Entities.ProductStatuses.Active, Category = categories[0], CreatedAt = DateTime.UtcNow.AddDays(-22) },
            new() { Name = "Monitor UltraWide 34\"", Description = "Monitor curvo 34 pulgadas QHD 144Hz para productividad.", Price = 599.00m, Stock = 12, Status = Entities.ProductStatuses.Active, Category = categories[0], CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new() { Name = "Ratón Inalámbrico Glide", Description = "Ratón ergonómico inalámbrico con sensor de 16000 DPI.", Price = 59.99m, Stock = 48, Status = Entities.ProductStatuses.Active, Category = categories[1], CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new() { Name = "Sudadera StorePro Hoodie", Description = "Sudadera con capucha de algodón orgánico, edición limitada.", Price = 45.00m, Stock = 5, Status = Entities.ProductStatuses.Active, Category = categories[2], CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new() { Name = "Lámpara Escritorio Lumen", Description = "Lámpara LED regulable con carga inalámbrica integrada.", Price = 39.90m, Stock = 30, Status = Entities.ProductStatuses.Active, Category = categories[3], CreatedAt = DateTime.UtcNow.AddDays(-7) }
        };

        context.Users.AddRange(users);
        context.Categories.AddRange(categories);
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        logger.LogInformation("Datos iniciales creados: {Users} usuarios, {Categories} categorías, {Products} productos.",
            users.Count, categories.Count, products.Count);
    }
}
