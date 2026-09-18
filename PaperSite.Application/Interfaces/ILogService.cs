using PaperSite.Domain.Entities;

namespace PaperSite.Application.Interfaces;

public interface ILogService
{
    Task AddAsync(Log log, CancellationToken cancellationToken = default);
}
