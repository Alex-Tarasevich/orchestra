using Orchestra.Domain.Entities;

namespace Orchestra.Application.Common.Interfaces;

/// <summary>
/// Service for caching ticket status and priority lookups.
/// Reduces database queries by caching status and priority objects in memory with configurable TTL.
/// </summary>
public interface ITicketLookupCacheService
{
    /// <summary>
    /// Gets all ticket statuses with in-memory caching (5-minute TTL).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all ticket statuses</returns>
    Task<List<TicketStatus>> GetAllStatusesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all ticket priorities with in-memory caching (5-minute TTL).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all ticket priorities</returns>
    Task<List<TicketPriority>> GetAllPrioritiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the status cache, forcing a refresh on the next call.
    /// </summary>
    Task InvalidateStatusCacheAsync();

    /// <summary>
    /// Invalidates the priority cache, forcing a refresh on the next call.
    /// </summary>
    Task InvalidatePriorityCacheAsync();
}
