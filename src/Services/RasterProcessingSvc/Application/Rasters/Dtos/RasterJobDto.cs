using RasterProcessingSvc.Domain.Rasters;

namespace RasterProcessingSvc.Application.Rasters.Dtos;

public sealed class RasterJobDto
{
    public Guid Id { get; init; }
    public string SourceFilePath { get; init; } = default!;
    public string? GeoJsonLocation { get; init; }
    public string Status { get; init; } = default!;
    public string? ErrorMessage { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }

    public static RasterJobDto FromEntity(RasterJob job) =>
        new()
        {
            Id = job.Id,
            SourceFilePath = job.SourceFilePath,
            GeoJsonLocation = job.GeoJsonLocation,
            Status = job.Status.ToString(),
            ErrorMessage = job.ErrorMessage,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt
        };
}
