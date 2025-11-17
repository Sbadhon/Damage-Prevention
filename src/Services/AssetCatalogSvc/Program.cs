using Marten;

namespace AssetCatalogSvc;

// Document model (stored in Postgres by Marten)
public record Asset(Guid Id, string Name, string Type, double Lat, double Lon);

// DTO for create
public record CreateAssetRequest(string Name, string Type, double Lat, double Lon);

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Read Postgres connection string from appsettings.json
        var connString = builder.Configuration.GetConnectionString("Postgres");

        // Register Marten (NO AutoCreate for now)
        builder.Services.AddMarten(options =>
        {
            options.Connection(connString);
            // We are intentionally NOT using Weasel.AutoCreate here
        });

        var app = builder.Build();

        // POST /assets  -> create asset (save to Postgres via Marten)
        app.MapPost("/assets", async (CreateAssetRequest req, IDocumentSession session) =>
        {
            var asset = new Asset(Guid.NewGuid(), req.Name, req.Type, req.Lat, req.Lon);
            session.Store(asset);
            await session.SaveChangesAsync();

            return Results.Created($"/assets/{asset.Id}", asset);
        });

        // GET /assets  -> list all
        app.MapGet("/assets", async (IQuerySession query) =>
        {
            var all = await query.Query<Asset>().ToListAsync();
            return Results.Ok(all);
        });

        // GET /assets/{id}  -> get one
        app.MapGet("/assets/{id:guid}", async (Guid id, IQuerySession query) =>
        {
            var asset = await query.LoadAsync<Asset>(id);
            return asset is not null ? Results.Ok(asset) : Results.NotFound();
        });

        // health
        app.MapGet("/ping", () => Results.Ok(new { service = "AssetCatalogSvc", status = "ok" }));

        app.Run();
    }
}
