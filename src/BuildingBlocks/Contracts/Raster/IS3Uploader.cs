namespace BuildingBlocks.Contracts
{
    public interface IS3Uploader
    {
        Task UploadFileAsync(string filePath, string bucket, string key);
    }

    public record RasterUploadedEvent(string RasterPath, string GeoJsonPath, DateTime UploadedAt);

}
