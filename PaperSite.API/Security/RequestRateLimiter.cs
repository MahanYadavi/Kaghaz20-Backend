using System.Threading.RateLimiting;

namespace PaperSite.API.Security;

// Middleware may retry an unsuccessful synchronous acquisition asynchronously.
// Reuse the rejection for this request rather than charging its other quotas again.
public sealed class RequestRateLimiter(PartitionedRateLimiter<HttpContext> inner) : PartitionedRateLimiter<HttpContext>
{
    private readonly object key = new();
    public override RateLimiterStatistics? GetStatistics(HttpContext resource) => inner.GetStatistics(resource);
    protected override RateLimitLease AttemptAcquireCore(HttpContext resource, int permitCount)
    {
        if (resource.Items.TryGetValue(key, out var saved)) return new Rejection((TimeSpan)saved!);
        var lease = inner.AttemptAcquire(resource, permitCount);
        if (!lease.IsAcquired)
            resource.Items[key] = lease.TryGetMetadata(MetadataName.RetryAfter, out var retry) ? retry : TimeSpan.FromSeconds(60);
        return lease;
    }
    protected override ValueTask<RateLimitLease> AcquireAsyncCore(HttpContext resource, int permitCount, CancellationToken cancellationToken)
        => ValueTask.FromResult(AttemptAcquireCore(resource, permitCount));
    protected override void Dispose(bool disposing) { if (disposing) inner.Dispose(); base.Dispose(disposing); }
    private sealed class Rejection(TimeSpan retry) : RateLimitLease
    {
        public override bool IsAcquired => false;
        public override IEnumerable<string> MetadataNames => [MetadataName.RetryAfter.Name];
        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            metadata = metadataName == MetadataName.RetryAfter.Name ? retry : null;
            return metadata is not null;
        }
    }
}
