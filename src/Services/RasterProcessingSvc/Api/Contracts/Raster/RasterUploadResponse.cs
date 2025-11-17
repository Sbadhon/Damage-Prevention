namespace RasterProcessingSvc.Api.Contracts.Raster;

public sealed record RasterUploadResponse(
    Guid JobId,
    string SourceFilePath,
    string Status,
    string? GeoJsonLocation,
    string? ErrorMessage
);
