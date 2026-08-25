using Microsoft.EntityFrameworkCore;
using StorePro.Api.Data;
using StorePro.Api.Repositories.Interfaces;

namespace StorePro.Api.Repositories;

public class GenericRepository<T>(StoreProDbContext context) : IGenericRepository<T> where T : class
{
    protected readonly StoreProDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(int id) => await DbSet.FindAsync(id);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync() => await DbSet.AsNoTracking().ToListAsync();

    public virtual async Task AddAsync(T entity) => await DbSet.AddAsync(entity);

    public virtual void Update(T entity) => DbSet.Update(entity);

    public virtual void Remove(T entity) => DbSet.Remove(entity);
}

public class ProductRepository(StoreProDbContext context)
    : GenericRepository<Entities.Product>(context), IProductRepository
{
    public async Task<(IReadOnlyList<Entities.Product> Items, int TotalCount)> GetPagedAsync(
        string? search, int? categoryId, string? status, int page, int pageSize)
    {
        var query = DbSet.Include(p => p.Category).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{term}%")
                                     || (p.Description != null && EF.Functions.Like(p.Description, $"%{term}%")));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<Entities.Product?> GetByIdWithCategoryAsync(int id)
        => DbSet.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

    public Task<int> CountAsync(string? status = null, int? maxStock = null)
    {
        var query = DbSet.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);
        if (maxStock.HasValue)
            query = query.Where(p => p.Stock <= maxStock.Value);
        return query.CountAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var normalized = name.Trim();
        return await DbSet.AnyAsync(p => p.Name.ToLower() == normalized.ToLower()
                                         && (excludeId == null || p.Id != excludeId));
    }
}

public class CategoryRepository(StoreProDbContext context)
    : GenericRepository<Entities.Category>(context), ICategoryRepository
{
    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var normalized = name.Trim();
        return await DbSet.AnyAsync(c => c.Name.ToLower() == normalized.ToLower()
                                         && (excludeId == null || c.Id != excludeId));
    }

    public Task<int> CountProductsAsync(int categoryId)
        => Context.Products.CountAsync(p => p.CategoryId == categoryId);
}

public class UserRepository(StoreProDbContext context)
    : GenericRepository<Entities.User>(context), IUserRepository
{
    public Task<Entities.User?> GetByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLower();
        return DbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == normalized);
    }

    public async Task<(IReadOnlyList<Entities.User> Items, int TotalCount)> GetPagedAsync(
        string? search, string? role, string? status, int page, int pageSize)
    {
        var query = DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => EF.Functions.Like(u.FullName, $"%{term}%")
                                     || EF.Functions.Like(u.Email, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(u => u.Status == status);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<int> CountAsync(string? role = null, string? status = null)
    {
        var query = DbSet.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(u => u.Status == status);
        return query.CountAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        var normalized = email.Trim().ToLower();
        return await DbSet.AnyAsync(u => u.Email.ToLower() == normalized
                                         && (excludeId == null || u.Id != excludeId));
    }
}

public class UnitOfWork(StoreProDbContext context,
                       IProductRepository products,
                       ICategoryRepository categories,
                       IUserRepository users) : IUnitOfWork
{
    public StoreProDbContext Context { get; } = context;
    public IProductRepository Products { get; } = products;
    public ICategoryRepository Categories { get; } = categories;
    public IUserRepository Users { get; } = users;

    public Task<int> SaveChangesAsync() => Context.SaveChangesAsync();
}
