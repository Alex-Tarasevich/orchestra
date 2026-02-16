using Orchestra.Application.Tickets.DTOs;
using Orchestra.Domain.Entities;

namespace Orchestra.Application.Common.Interfaces;

/// <summary>
/// Service for fetching tickets from external providers.
/// Manages multi-provider pagination and round-robin distribution.
/// </summary>
public interface IExternalTicketFetchingService
{
    /// <summary>
    /// Fetches tickets from multiple provider integrations with fair distribution.
    /// Handles pagination state across providers.
    /// </summary>
    /// <param name="trackerIntegrations">List of TRACKER integrations to fetch from</param>
    /// <param name="slotsToFill">Number of ticket slots to fill</param>
    /// <param name="currentState">Current pagination state from previous page (if any)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fetched tickets, hasMore flag, and updated pagination state</returns>
    Task<(List<TicketDto> Tickets, bool HasMore, ExternalPaginationState State)> FetchExternalTicketsAsync(
        List<Integration> trackerIntegrations,
        int slotsToFill,
        ExternalPaginationState? currentState,
        CancellationToken cancellationToken);

    /// <summary>
    /// Calculates round-robin distribution of remaining slots across integrations.
    /// Ensures fair distribution when multiple providers are available.
    /// </summary>
    Dictionary<Guid, int> CalculateProviderDistribution(
        List<Integration> integrations,
        int remainingSlots);
}
