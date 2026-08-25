namespace StorePro.Api.Entities;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string Status { get; set; } = ProductStatuses.Active;

    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class ProductStatuses
{
    public const string Active = "Active";
    public const string Draft = "Draft";
    public const string OutOfStock = "OutOfStock";

    public static readonly IReadOnlyList<string> All = [Active, Draft, OutOfStock];
}
