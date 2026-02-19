using Orchestra.Application.Tickets.DTOs;

namespace Orchestra.Application.Common.Interfaces;

/// <summary>
/// Service for querying tickets (read-only operations).
/// Handles retrieval of internal and external tickets, status/priority lookups, 
/// and AI-generated summaries.
/// </summary>
public interface ITicketQueryService
{
    /// <summary>
    /// Retrieves a paginated list of tickets for a workspace.
    /// Fetches both internal tickets from DB and external tickets from providers.
    /// </summary>
    Task<PaginatedTicketsResponse> GetTicketsAsync(
        Guid workspaceId,
        Guid userId,
        string? pageToken = null,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single ticket by ID (supports GUID for internal, composite for external).
    /// </summary>
    Task<TicketDto> GetTicketByIdAsync(
        string ticketId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all ticket statuses.
    /// </summary>
    Task<List<TicketStatusDto>> GetAllStatusesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all ticket priorities.
    /// </summary>
    Task<List<TicketPriorityDto>> GetAllPrioritiesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an AI-powered summary for a ticket.
    /// </summary>
}
