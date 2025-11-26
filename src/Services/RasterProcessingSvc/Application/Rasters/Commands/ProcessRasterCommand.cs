using MediatR;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using RasterProcessingSvc.Application.Rasters.Dtos;
using RasterProcessingSvc.Domain.Geo;
using RasterProcessingSvc.Domain.Rasters;

namespace RasterProcessingSvc.Application.Rasters.Commands;

public sealed class ProcessRasterCommand : IRequest<RasterJobDto>
{
    public string SourceFilePath { get; init; } = default!;
}

public sealed class ProcessRasterCommandHandler
    : IRequestHandler<ProcessRasterCommand, RasterJobDto>
{
    private readonly IRasterReader _reader;
    private readonly IGeoJsonStorage _storage;
    private readonly IRasterJobRepository _jobs;

    public ProcessRasterCommandHandler(
        IRasterReader reader,
        IGeoJsonStorage storage,
        IRasterJobRepository jobs)
    {
        _reader = reader;
        _storage = storage;
        _jobs = jobs;
    }

    public async Task<RasterJobDto> Handle(
        ProcessRasterCommand request,
        CancellationToken ct)
    {
        var job = RasterJob.Create(request.SourceFilePath, DateTimeOffset.UtcNow);
        await _jobs.AddAsync(job, ct);

        try
        {
            var raster = _reader.ReadRaster(request.SourceFilePath);
            var geoJsonPath = GenerateGeoJson(raster);

            var location = await _storage.StoreAsync(geoJsonPath, ct);

            job.MarkCompleted(location, DateTimeOffset.UtcNow);
            await _jobs.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            job.MarkFailed(ex.Message, DateTimeOffset.UtcNow);
            await _jobs.SaveChangesAsync(ct);
        }

        return RasterJobDto.FromEntity(job);
    }

    private static string GenerateGeoJson(Raster raster)
    {
        var featureCollection = new FeatureCollection();

        foreach (var poly in raster.Polygons)
        {
            var feature = new NetTopologySuite.Features.Feature(
                poly.Geometry,
                new AttributesTable());

            foreach (var kvp in poly.Attributes)
            {
                feature.Attributes.Add(kvp.Key, kvp.Value);
            }

            featureCollection.Add(feature);
        }

        var fileName = $"raster-{Guid.NewGuid():N}.geojson";
        var geoJsonPath = Path.Combine(Path.GetTempPath(), fileName);

        var serializer = GeoJsonSerializer.Create();

        using (var writer = new StreamWriter(geoJsonPath))
        {
            serializer.Serialize(writer, featureCollection);
        }

        return geoJsonPath;
    }
}
