namespace RasterProcessingSvc.Domain.Rasters;

public interface IRasterJobRepository
{
    Task<RasterJob?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(RasterJob job, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
