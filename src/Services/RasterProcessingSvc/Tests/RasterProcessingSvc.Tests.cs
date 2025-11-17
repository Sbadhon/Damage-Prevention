using RasterProcessingSvc.Domain;
using RasterProcessingSvc.Infrastructure;
using System.IO;
using System.Threading.Tasks;


namespace RasterProcessingSvc.Tests
{
    public class UploadRasterTests
    {
      
        public async Task ProcessRaster_ShouldGenerateGeoJson()
        {
           
            var tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, "dummy");

       
            var content = await File.ReadAllTextAsync(tempFile);
        
        }
    }
}
