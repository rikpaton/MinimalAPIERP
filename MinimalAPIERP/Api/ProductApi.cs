using ERP.Data;
using ERP.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERP.Api;

internal static class ProductApi
{
    public static RouteGroupBuilder MapProductApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/erp")
            .WithTags("Product Api");


        // TODO: Mover a config
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            //PropertyNameCaseInsensitive = false,
            //PropertyNamingPolicy = null,
            WriteIndented = true,
            //IncludeFields = false,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            //ReferenceHandler = ReferenceHandler.Preserve
        };

        group.MapGet("/products", async (AppDbContext db) =>
    await db.Products.ToListAsync() is IList<Product> products
        ? Results.Json(products, options)
        : Results.NotFound())
    .WithOpenApi();

        group.MapGet("/products/{id}", async (AppDbContext db, Guid id) =>
            await db.Products.FindAsync(id) is Product product
                ? Results.Json(product, options)
                : Results.NotFound())
            .WithOpenApi();

        group.MapPost("/products", async (AppDbContext db, Product product) =>
        {
            product.Guid = Guid.NewGuid();
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return Results.Created($"/products/{product.ProductId}", product);
        }).WithOpenApi();

        group.MapPut("/products/{id}", async (AppDbContext db, Guid id, Product updatedProduct) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();

            product.SkuNumber = updatedProduct.SkuNumber;
            product.CategoryId = updatedProduct.CategoryId;
            product.RecommendationId = updatedProduct.RecommendationId;
            product.Title = updatedProduct.Title;
            product.Price = updatedProduct.Price;
            product.SalePrice = updatedProduct.SalePrice;
            product.ProductArtUrl = updatedProduct.ProductArtUrl;
            product.Description = updatedProduct.Description;
            product.Created = updatedProduct.Created;
            product.ProductDetails = updatedProduct.ProductDetails;
            product.Inventory = updatedProduct.Inventory;
            product.LeadTime = updatedProduct.LeadTime;

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithOpenApi();

        group.MapDelete("/products/{id}", async (AppDbContext db, Guid id) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();

            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithOpenApi();

        return group;
    }
}
