using System;
using NetTopologySuite.Geometries;
using RasterProcessingSvc.Domain.Geo;
using RasterProcessingSvc.Domain.Rasters;

namespace RasterProcessingSvc.Infrastructure.Geo;

public sealed class MockRasterReader : IRasterReader
{
    public Raster ReadRaster(string filePath)
    {
        var geomFactory = new GeometryFactory();

        var polygon = geomFactory.CreatePolygon(new[]
        {
            new Coordinate(0, 0),
            new Coordinate(0, 10),
            new Coordinate(10, 10),
            new Coordinate(10, 0),
            new Coordinate(0, 0)
        });

        var raster = new Raster();
        raster.Polygons.Add(new RasterPolygon
        {
            Geometry = polygon,
            Attributes =
            {
                ["sourcePath"] = filePath,
                ["note"] = "Mock raster polygon"
            }
        });

        Console.WriteLine($"[MockRasterReader] Read raster from {filePath} -> 1 polygon");
        return raster;
    }
}
