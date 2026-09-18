using PaperSite.Application.Interfaces;
using PaperSite.Domain.Entities;
using PaperSite.Infrastructure.Persistence;

namespace PaperSite.Infrastructure.Services;

public class LogService : ILogService
{
    private readonly ApplicationDbContext _context;

    public LogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Log log, CancellationToken cancellationToken = default)
    {
        log.CreatedAt = DateTime.Now;
        log.UpdatedAt = DateTime.Now;

        await _context.Logs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
