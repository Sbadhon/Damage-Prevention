namespace RasterProcessingSvc.Domain.Rasters;

public sealed class RasterJob
{
    public Guid Id { get; private set; }
    public string SourceFilePath { get; private set; } = default!;
    public string? GeoJsonLocation { get; private set; }
    public RasterJobStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private RasterJob() { }

    private RasterJob(Guid id, string sourceFilePath, DateTimeOffset createdAt)
    {
        Id = id;
        SourceFilePath = sourceFilePath;
        CreatedAt = createdAt;
        Status = RasterJobStatus.Pending;
    }

    public static RasterJob Create(string sourceFilePath, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
            throw new ArgumentException("Source file path is required.", nameof(sourceFilePath));

        return new RasterJob(Guid.NewGuid(), sourceFilePath, now);
    }

    public void MarkCompleted(string geoJsonLocation, DateTimeOffset now)
    {
        GeoJsonLocation = geoJsonLocation;
        Status = RasterJobStatus.Completed;
        CompletedAt = now;
        ErrorMessage = null;
    }

    public void MarkFailed(string errorMessage, DateTimeOffset now)
    {
        ErrorMessage = errorMessage;
        Status = RasterJobStatus.Failed;
        CompletedAt = now;
    }
}
