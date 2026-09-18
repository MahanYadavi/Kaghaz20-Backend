using System.Diagnostics;
using System.Threading.RateLimiting;

namespace PaperSite.API.Security;

// A rolling cooldown measured from the last accepted request, including concurrent requests.
public sealed class CooldownLimiter(TimeSpan cooldown) : RateLimiter
{
    private readonly object gate = new();
    private long? lastAccepted;
    private readonly long created = Stopwatch.GetTimestamp();

    public override TimeSpan? IdleDuration
    {
        get { lock (gate) return lastAccepted is null ? Stopwatch.GetElapsedTime(created)
            : Stopwatch.GetElapsedTime(lastAccepted.Value) >= cooldown
                ? Stopwatch.GetElapsedTime(lastAccepted.Value) - cooldown : null; }
    }

    public override RateLimiterStatistics? GetStatistics() => null;

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        lock (gate)
        {
            var remaining = lastAccepted is null ? TimeSpan.Zero
                : cooldown - Stopwatch.GetElapsedTime(lastAccepted.Value);
            if (remaining > TimeSpan.Zero) return new Lease(false, remaining);
            if (permitCount > 0) lastAccepted = Stopwatch.GetTimestamp();
            return new Lease(true, TimeSpan.Zero);
        }
    }

    protected override ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
        => ValueTask.FromResult(AttemptAcquireCore(permitCount));

    private sealed class Lease(bool acquired, TimeSpan retryAfter) : RateLimitLease
    {
        public override bool IsAcquired => acquired;
        public override IEnumerable<string> MetadataNames => [MetadataName.RetryAfter.Name];
        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            metadata = metadataName == MetadataName.RetryAfter.Name ? retryAfter : null;
            return metadata is not null;
        }
    }
}
