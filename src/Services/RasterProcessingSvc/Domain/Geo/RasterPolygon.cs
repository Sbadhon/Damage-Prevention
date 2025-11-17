using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace RasterProcessingSvc.Domain.Geo;

public sealed class RasterPolygon
{
    public Geometry Geometry { get; init; } = default!;
    public IDictionary<string, object> Attributes { get; init; } = new Dictionary<string, object>();
}
