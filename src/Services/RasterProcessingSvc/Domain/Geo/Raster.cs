using System.Collections.Generic;

namespace RasterProcessingSvc.Domain.Geo;

public sealed class Raster
{
    public List<RasterPolygon> Polygons { get; } = new();
}
