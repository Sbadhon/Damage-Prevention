using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RasterProcessingSvc.Domain.Rasters;

namespace RasterProcessingSvc.Infrastructure.Storage;

public sealed class LocalGeoJsonStorage : IGeoJsonStorage
{
    private readonly string _rootFolder;

    public LocalGeoJsonStorage()
    {
        _rootFolder = Path.Combine(Path.GetTempPath(), "raster-geojson");
        Directory.CreateDirectory(_rootFolder);
    }

    public Task<string> StoreAsync(string geoJsonFilePath, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(geoJsonFilePath);
        var destPath = Path.Combine(_rootFolder, fileName);

        File.Copy(geoJsonFilePath, destPath, overwrite: true);

        Console.WriteLine($"[LocalGeoJsonStorage] Stored {geoJsonFilePath} -> {destPath}");

        // In a real system, you’d return an S3/Blob URL. Here we return the local path.
        return Task.FromResult(destPath);
    }
}
