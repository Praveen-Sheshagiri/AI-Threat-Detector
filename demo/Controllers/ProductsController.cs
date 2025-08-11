using Microsoft.AspNetCore.Mvc;
using ThreatDetector.Demo.Services;
using ThreatDetector.Demo.Data;
using ThreatDetector.SDK;

namespace ThreatDetector.Demo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IThreatDetectorClient _threatDetectorClient;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IThreatDetectorClient threatDetectorClient,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _threatDetectorClient = threatDetectorClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
        [FromQuery] string? category = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
    {
        try
        {
            // Analyze search parameters for potential threats
            var searchData = new
            {
                category = category ?? "",
                minPrice = minPrice ?? 0,
                maxPrice = maxPrice ?? 0,
                searchType = "ProductSearch",
                userAgent = Request.Headers.UserAgent.ToString(),
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            var threatScore = await _threatDetectorClient.GetThreatScoreAsync(searchData);
            _logger.LogInformation("Product search threat score: {ThreatScore}", threatScore);

            var products = await _productService.GetProductsAsync(category, minPrice, maxPrice);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { error = $"Product with ID {id} not found" });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new product (Admin only - demo purposes)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            // Analyze product creation for potential threats (e.g., malicious content)
            var creationData = new
            {
                productName = request.Name,
                productDescription = request.Description,
                price = request.Price,
                category = request.Category,
                action = "ProductCreation",
                contentLength = (request.Name + request.Description).Length,
                userAgent = Request.Headers.UserAgent.ToString(),
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            var threatResult = await _threatDetectorClient.AnalyzeThreatAsync(creationData, "ProductCreation");
            
            if (threatResult.IsThreat && threatResult.ThreatScore > 0.8)
            {
                _logger.LogWarning("High-risk product creation attempt blocked. Score: {ThreatScore}", threatResult.ThreatScore);
                return BadRequest(new { error = "Product creation request flagged as suspicious", threatId = threatResult.ThreatId });
            }

            var product = await _productService.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Search products (potentially vulnerable to injection attacks - for demo)
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromBody] SearchRequest request)
    {
        try
        {
            // This endpoint demonstrates potential vulnerability detection
            var searchData = new
            {
                searchQuery = request.Query,
                searchType = "AdvancedProductSearch",
                queryLength = request.Query?.Length ?? 0,
                containsSqlKeywords = ContainsSqlKeywords(request.Query),
                containsScriptTags = ContainsScriptTags(request.Query),
                userAgent = Request.Headers.UserAgent.ToString(),
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                timestamp = DateTime.UtcNow
            };

            // Analyze for SQL injection, XSS, and other threats
            var threatResult = await _threatDetectorClient.AnalyzeThreatAsync(searchData, "SearchQuery");
            
            if (threatResult.IsThreat)
            {
                _logger.LogWarning("Suspicious search query detected: {Query}. Threat: {ThreatType}, Score: {ThreatScore}", 
                    request.Query, threatResult.ThreatType, threatResult.ThreatScore);
                
                // For high-threat queries, return sanitized results or block entirely
                if (threatResult.ThreatScore > 0.9)
                {
                    return BadRequest(new { 
                        error = "Search query blocked due to security concerns", 
                        threatId = threatResult.ThreatId,
                        threatType = threatResult.ThreatType 
                    });
                }
            }

            var products = await _productService.SearchProductsAsync(request.Query ?? "");
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Bulk update products (potential for abuse - for demo)
    /// </summary>
    [HttpPut("bulk")]
    public async Task<ActionResult> BulkUpdateProducts([FromBody] BulkUpdateRequest request)
    {
        try
        {
            // Analyze bulk operations for potential abuse
            var bulkData = new
            {
                operationType = "BulkUpdate",
                productCount = request.ProductIds?.Count ?? 0,
                updateFields = request.UpdateData?.Keys.ToList() ?? new List<string>(),
                userAgent = Request.Headers.UserAgent.ToString(),
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                timestamp = DateTime.UtcNow,
                requestSize = System.Text.Json.JsonSerializer.Serialize(request).Length
            };

            var threatResult = await _threatDetectorClient.AnalyzeThreatAsync(bulkData, "BulkOperation");
            
            if (threatResult.IsThreat)
            {
                _logger.LogWarning("Suspicious bulk operation detected. Threat: {ThreatType}, Score: {ThreatScore}", 
                    threatResult.ThreatType, threatResult.ThreatScore);
                
                if (threatResult.ThreatScore > 0.75)
                {
                    return BadRequest(new { 
                        error = "Bulk operation blocked due to security concerns", 
                        threatId = threatResult.ThreatId 
                    });
                }
            }

            var result = await _productService.BulkUpdateProductsAsync(request.ProductIds ?? new List<int>(), request.UpdateData ?? new Dictionary<string, object>());
            return Ok(new { message = $"Successfully updated {result} products" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk update operation");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private static bool ContainsSqlKeywords(string? query)
    {
        if (string.IsNullOrEmpty(query)) return false;
        
        var sqlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "UNION", "OR 1=1", "--", "/*", "*/" };
        return sqlKeywords.Any(keyword => query.ToUpper().Contains(keyword));
    }

    private static bool ContainsScriptTags(string? query)
    {
        if (string.IsNullOrEmpty(query)) return false;
        
        return query.ToLower().Contains("<script") || query.ToLower().Contains("javascript:");
    }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SearchRequest
{
    public string? Query { get; set; }
}

public class BulkUpdateRequest
{
    public List<int>? ProductIds { get; set; }
    public Dictionary<string, object>? UpdateData { get; set; }
}
