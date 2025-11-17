namespace RasterProcessingSvc.Domain.Rasters;

public interface IGeoJsonStorage
{
    /// <summary>
    /// Persist a GeoJSON file and return a stable reference (e.g. URL, path, key).
    /// </summary>
    Task<string> StoreAsync(string geoJsonFilePath, CancellationToken ct = default);
}
