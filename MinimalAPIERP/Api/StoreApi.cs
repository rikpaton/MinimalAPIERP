using ERP.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERP.Api;

internal static class StoreApi
{
    public static RouteGroupBuilder MapStoreApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/erp")
            .WithTags("Store Api");

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        routes.MapGet("/stores", async (AppDbContext db) =>
            await db.Stores.ToListAsync() is IList<Store> stores
                 ? Results.Json(stores, options)
                 : Results.NotFound())
            .WithOpenApi();

        routes.MapGet("/stores/{id}", async (AppDbContext db, Guid id) =>
            await db.Stores.FindAsync(id) is Store store
                ? Results.Json(store, options)
                : Results.NotFound())
            .WithOpenApi();

        routes.MapPost("/stores", async (AppDbContext db, Store store) =>
        {
            store.Guid = Guid.NewGuid();
            db.Stores.Add(store);
            await db.SaveChangesAsync();
            return Results.Created($"/stores/{store.StoreId}", store);
        }).WithOpenApi();

        routes.MapPut("/stores/{id}", async (AppDbContext db, Guid id, Store updatedStore) =>
        {
            var store = await db.Stores.FindAsync(id);
            if (store is null) return Results.NotFound();

            store.Name = updatedStore.Name;
            store.Rainchecks = updatedStore.Rainchecks;

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithOpenApi();

        routes.MapDelete("/stores/{id}", async (AppDbContext db, Guid id) =>
        {
            var store = await db.Stores.FindAsync(id);
            if (store is null) return Results.NotFound();

            db.Stores.Remove(store);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithOpenApi();

        return group;
    }
}
