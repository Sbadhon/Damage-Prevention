using System;
using System.IO;

namespace RasterProcessingSvc.Utilities
{
    public static class CreateSampleTif
    {
        public static string Generate(string fileName = "sample.tif")
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            // Just write some dummy content
            File.WriteAllText(path, "This is a dummy raster file for testing.");

            Console.WriteLine($"Dummy raster created at: {path}");
            return path;
        }
    }
}
