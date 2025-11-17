namespace BuildingBlocks.SharedKernel
{
    public class Raster
    {
        public string FilePath { get; }
        public int Width { get; }
        public int Height { get; }
        public double MinX { get; }
        public double MaxX { get; }
        public double MinY { get; }
        public double MaxY { get; }

        public Raster(string filePath, int width, int height, double minX, double maxX, double minY, double maxY)
        {
            FilePath = filePath;
            Width = width;
            Height = height;
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
        }
    }
}
