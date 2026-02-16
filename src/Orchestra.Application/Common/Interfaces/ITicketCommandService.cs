using Orchestra.Application.Tickets.DTOs;

namespace Orchestra.Application.Common.Interfaces;

/// <summary>
/// Service for ticket command operations (write operations).
/// Handles creation, updates, deletion, and external ticket conversion.
/// </summary>
public interface ITicketCommandService
{
    /// <summary>
    /// Creates a new internal ticket.
    /// </summary>
    Task<TicketDto> CreateTicketAsync(
        Guid userId,
        CreateTicketRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a ticket's properties (status, priority, assignments, description).
    /// Materializes external tickets on first assignment.
    /// </summary>
    Task<TicketDto> UpdateTicketAsync(
        string ticketId,
        Guid userId,
        UpdateTicketRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts an internal ticket to an external tracker ticket.
    /// Creates the issue in the external system and updates the internal record.
    /// </summary>
    Task<TicketDto> ConvertToExternalAsync(
        string ticketId,
        Guid userId,
        Guid integrationId,
        string issueTypeName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an internal ticket (external tickets cannot be deleted).
    /// </summary>
    Task DeleteTicketAsync(
        string ticketId,
        Guid userId,
        CancellationToken cancellationToken = default);

}
