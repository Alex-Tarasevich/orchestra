using Orchestra.Application.Tickets.DTOs;
using Orchestra.Domain.Entities;
using Orchestra.Domain.Enums;

namespace Orchestra.Application.Common.Interfaces;

/// <summary>
/// Service for materializing external tickets into internal database records.
/// Handles conversion of external ticket metadata to internal format and priority mapping.
/// </summary>
public interface ITicketMaterializationService
{
    /// <summary>
    /// Materializes an external ticket record in the database.
    /// Maps external priority to internal priority scale.
    /// </summary>
    /// <param name="integrationId">The integration ID from external provider</param>
    /// <param name="externalTicketId">The external ticket ID from provider</param>
    /// <param name="workspaceId">The workspace ID for the materialized ticket</param>
    /// <param name="externalTicket">The external ticket data from provider</param>
    /// <param name="assignedAgentId">Optional agent assignment</param>
    /// <param name="assignedWorkflowId">Optional workflow assignment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created internal ticket entity</returns>
    Task<Ticket> MaterializeFromExternalAsync(
        Guid integrationId,
        string externalTicketId,
        Guid workspaceId,
        ExternalTicketDto externalTicket,
        Guid? assignedAgentId,
        Guid? assignedWorkflowId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Maps external priority value to closest internal priority.
    /// Uses nearest-neighbor matching based on priority value.
    /// </summary>
    Task<TicketPriority> MapExternalPriorityToInternalAsync(
        int externalPriorityValue,
        CancellationToken cancellationToken);
}
