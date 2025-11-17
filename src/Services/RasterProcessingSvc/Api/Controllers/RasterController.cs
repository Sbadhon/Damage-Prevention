using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RasterProcessingSvc.Api.Contracts.Raster;
using RasterProcessingSvc.Application.Rasters.Commands;
using RasterProcessingSvc.Application.Rasters.Dtos;

namespace RasterProcessingSvc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RasterController : ControllerBase
{
    private readonly ISender _sender;

    public RasterController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("File is required.");

        var tempPath = Path.Combine(Path.GetTempPath(), file.FileName);

        await using (var stream = System.IO.File.Create(tempPath))
        {
            await file.CopyToAsync(stream, ct);
        }

        var cmd = new ProcessRasterCommand
        {
            SourceFilePath = tempPath
        };

        RasterJobDto dto = await _sender.Send(cmd, ct);

        var response = new RasterUploadResponse(
            JobId: dto.Id,
            SourceFilePath: dto.SourceFilePath,
            Status: dto.Status,
            GeoJsonLocation: dto.GeoJsonLocation,
            ErrorMessage: dto.ErrorMessage
        );

        return Ok(response);
    }
}
