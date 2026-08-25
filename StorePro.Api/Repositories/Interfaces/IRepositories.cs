using StorePro.Api.Data;

namespace StorePro.Api.Repositories.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}

public interface IProductRepository : IGenericRepository<Entities.Product>
{
    Task<(IReadOnlyList<Entities.Product> Items, int TotalCount)> GetPagedAsync(
        string? search, int? categoryId, string? status, int page, int pageSize);
    Task<Entities.Product?> GetByIdWithCategoryAsync(int id);
    Task<int> CountAsync(string? status = null, int? maxStock = null);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
}

public interface ICategoryRepository : IGenericRepository<Entities.Category>
{
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<int> CountProductsAsync(int categoryId);
}

public interface IUserRepository : IGenericRepository<Entities.User>
{
    Task<Entities.User?> GetByEmailAsync(string email);
    Task<(IReadOnlyList<Entities.User> Items, int TotalCount)> GetPagedAsync(
        string? search, string? role, string? status, int page, int pageSize);
    Task<int> CountAsync(string? role = null, string? status = null);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
}

public interface IUnitOfWork
{
    StoreProDbContext Context { get; }
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync();
}
