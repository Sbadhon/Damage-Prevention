using MediatR;
using RasterProcessingSvc.Application.Rasters.Commands;
using RasterProcessingSvc.Domain.Rasters;
using RasterProcessingSvc.Infrastructure.Geo;
using RasterProcessingSvc.Infrastructure.Rasters;
using RasterProcessingSvc.Infrastructure.Storage;

namespace RasterProcessingSvc;

public class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // Controllers + Swagger
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // MediatR (if you keep it for this svc)
        builder.Services.AddMediatR(typeof(ProcessRasterCommand).Assembly);

        // Domain ports -> Infrastructure implementations
        builder.Services.AddSingleton<IRasterReader, MockRasterReader>();
        builder.Services.AddSingleton<IGeoJsonStorage, LocalGeoJsonStorage>();
        builder.Services.AddSingleton<IRasterJobRepository, InMemoryRasterJobRepository>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();

        app.Run();
    }
}