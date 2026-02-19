namespace Orchestra.Application.Common.Interfaces;

/// <summary>
/// Service for ticket-specific authorization checks.
/// Centralizes workspace membership and ticket access validation logic.
/// </summary>
public interface ITicketAuthorizationService
{
    /// <summary>
    /// Ensures user has access to a ticket's workspace.
    /// Throws UnauthorizedTicketAccessException if not authorized.
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <param name="ticket">The ticket entity to check access for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="UnauthorizedTicketAccessException">Thrown if user lacks workspace access</exception>
    Task EnsureTicketAccessAsync(Guid userId, Orchestra.Domain.Entities.Ticket ticket, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures user has access to an integration's workspace.
    /// Throws UnauthorizedTicketAccessException if not authorized.
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <param name="integration">The integration entity to check access for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="UnauthorizedTicketAccessException">Thrown if user lacks workspace access</exception>
    Task EnsureExternalTicketAccessAsync(Guid userId, Orchestra.Domain.Entities.Integration integration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures user can perform ticket workspace operations.
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <param name="workspaceId">Workspace ID to check access for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="UnauthorizedTicketAccessException">Thrown if user lacks workspace access</exception>
    Task EnsureWorkspaceAccessAsync(Guid userId, Guid workspaceId, CancellationToken cancellationToken = default);
}
