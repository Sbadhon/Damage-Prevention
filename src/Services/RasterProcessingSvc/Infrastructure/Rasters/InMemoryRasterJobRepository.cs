using System.Collections.Concurrent;
using RasterProcessingSvc.Domain.Rasters;

namespace RasterProcessingSvc.Infrastructure.Rasters;

public sealed class InMemoryRasterJobRepository : IRasterJobRepository
{
    private readonly ConcurrentDictionary<Guid, RasterJob> _store = new();

    public Task<RasterJob?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var job);
        return Task.FromResult(job);
    }

    public Task AddAsync(RasterJob job, CancellationToken ct = default)
    {
        _store[job.Id] = job;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        // In-memory, nothing to persist.
        return Task.CompletedTask;
    }
}
