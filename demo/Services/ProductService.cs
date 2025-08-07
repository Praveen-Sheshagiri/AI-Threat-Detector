using Microsoft.EntityFrameworkCore;
using ThreatDetector.Demo.Data;

namespace ThreatDetector.Demo.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetProductsAsync(string? category = null, decimal? minPrice = null, decimal? maxPrice = null);
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(CreateProductRequest request);
    Task<IEnumerable<Product>> SearchProductsAsync(string query);
    Task<int> BulkUpdateProductsAsync(List<int> productIds, Dictionary<string, object> updateData);
}

public class ProductService : IProductService
{
    private readonly DemoDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(DemoDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetProductsAsync(string? category = null, decimal? minPrice = null, decimal? maxPrice = null)
    {
        var query = _context.Products.Where(p => p.IsActive);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category.ToLower().Contains(category.ToLower()));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Category = request.Category,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new product: {ProductName} with ID {ProductId}", product.Name, product.Id);
        return product;
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return await GetProductsAsync();
        }

        // Simple search implementation - in real application, would use full-text search
        var products = await _context.Products
            .Where(p => p.IsActive && 
                       (p.Name.ToLower().Contains(query.ToLower()) ||
                        p.Description.ToLower().Contains(query.ToLower()) ||
                        p.Category.ToLower().Contains(query.ToLower())))
            .OrderBy(p => p.Name)
            .ToListAsync();

        return products;
    }

    public async Task<int> BulkUpdateProductsAsync(List<int> productIds, Dictionary<string, object> updateData)
    {
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        int updatedCount = 0;

        foreach (var product in products)
        {
            var isUpdated = false;

            if (updateData.TryGetValue("price", out var priceValue) && decimal.TryParse(priceValue.ToString(), out var price))
            {
                product.Price = price;
                isUpdated = true;
            }

            if (updateData.TryGetValue("category", out var categoryValue))
            {
                product.Category = categoryValue.ToString() ?? product.Category;
                isUpdated = true;
            }

            if (updateData.TryGetValue("isActive", out var isActiveValue) && bool.TryParse(isActiveValue.ToString(), out var isActive))
            {
                product.IsActive = isActive;
                isUpdated = true;
            }

            if (isUpdated)
            {
                updatedCount++;
            }
        }

        if (updatedCount > 0)
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Bulk updated {UpdatedCount} products", updatedCount);
        }

        return updatedCount;
    }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
