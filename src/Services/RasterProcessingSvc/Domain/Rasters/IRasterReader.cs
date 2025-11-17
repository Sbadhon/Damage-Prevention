using RasterProcessingSvc.Domain.Geo;

namespace RasterProcessingSvc.Domain.Rasters;

public interface IRasterReader
{
    Raster ReadRaster(string filePath);
}
