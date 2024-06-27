﻿using ERP.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using MinimalAPIERP.Dtos;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERP.Api;

internal static class RaincheckApi
{
    public static RouteGroupBuilder MapRaincheckApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/erp")
            .WithTags("Raincheck Api");

        // TODO: Mover a config
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            //PropertyNameCaseInsensitive = false,
            //PropertyNamingPolicy = null,
            WriteIndented = true,
            //IncludeFields = false,
            MaxDepth = 0,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            //ReferenceHandler = ReferenceHandler.Preserve
            //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        group.MapGet("/rainchecks", async Task<Results<Ok<IList<Raincheck>>, NotFound>> (AppDbContext db) =>
            await db.Rainchecks
                .Include(s => s.Product)
                .Include(s => s.Store)
                .ToListAsync()
                    is IList<Raincheck> rainchecks
                        ? TypedResults.Ok(rainchecks)
                        : TypedResults.NotFound())
        .WithOpenApi();

        group.MapGet("/rainchecksb", async (AppDbContext db, int pageSize = 10, int page = 0) =>
        {
            var data = await db.Rainchecks
                .OrderBy(s => s.RaincheckId)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Include(s => s.Product)
                    .ThenInclude(s => s.Category)
                .Include(s => s.Store)
                .Select(r => new { r.StoreId, r.Name, r.Product, r.Store })
                .ToListAsync();

            return data.Any()
                ? Results.Json(data, options)
                : Results.NotFound();
        })
        .WithOpenApi();

        group.MapGet("/rainchecksc", async (AppDbContext db, int pageSize = 10, int page = 0) =>
        {
            var data = await db.Rainchecks
                .OrderBy(s => s.RaincheckId)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Include(s => s.Product)
                .Include(s => s.Product.Category)
                .Include(s => s.Store)
                .Select(x => new {
                    Name = x.Name,
                    Count = x.Count,
                    SalePrice = x.SalePrice,
                    Store = new {
                        Name = x.Store.Name,
                    },
                    Product = new { 
                        Name = x.Product.Title, 
                        Category = new {
                            Name = x.Product.Category.Name
                        }
                    }
                })
                .ToListAsync();

            return data.Any()
                ? Results.Json(data, options)
                : Results.NotFound();
        })
        .WithOpenApi();

        group.MapGet("/rainchecksd", async Task<Results<Ok<List<RaincheckDto>>, NotFound>> (AppDbContext db, int pageSize = 10, int page = 0) =>
        {
            var data = await db.Rainchecks
                .OrderBy(s => s.RaincheckId)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Include(s => s.Product)
                .Include(s => s.Product.Category)
                .Include(s => s.Store)
                .Select(x => new RaincheckDto
                {
                    Name = x.Name,
                    Count = x.Count,
                    SalePrice = x.SalePrice,
                    Store = new StoreDto
                    {
                        Name = x.Store.Name
                    },
                    Product = new ProductDto
                    {
                        Name = x.Product.Title,
                        Category = new CategoryDto
                        {
                            Name = x.Product.Category.Name
                        }
                    }
                })
                .ToListAsync();

            return data.Any()
                ? TypedResults.Ok(data)
                : TypedResults.NotFound();

        })
        .WithOpenApi();


        return group;
    }
}
