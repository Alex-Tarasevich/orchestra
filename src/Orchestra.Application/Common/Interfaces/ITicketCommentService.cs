using Orchestra.Application.Tickets.DTOs;

namespace Orchestra.Application.Common.Interfaces;

/// <summary>
/// Service for managing ticket comments (internal and external).
/// Handles adding comments to both internal database-backed tickets and external provider-backed tickets.
/// </summary>
public interface ITicketCommentService
{
    /// <summary>
    /// Adds a comment to a ticket (internal or external). Routes to the correct method based on ticketId format.
    /// </summary>
    /// <param name="ticketId">Ticket ID (internal GUID or composite external ID)</param>
    /// <param name="userId">User ID</param>
    /// <param name="request">Comment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created comment DTO</returns>
    Task<CommentDto> AddCommentAsync(
        string ticketId,
        Guid userId,
        AddCommentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a comment to an internal ticket in the database.
    /// </summary>
    /// <param name="ticketId">The internal ticket ID (GUID format)</param>
    /// <param name="userId">The user ID adding the comment</param>
    /// <param name="request">Comment content request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created comment DTO</returns>
    /// <exception cref="TicketNotFoundException">Thrown if ticket not found</exception>
    /// <exception cref="UnauthorizedTicketAccessException">Thrown if user lacks workspace access</exception>
    Task<CommentDto> AddCommentToInternalTicketAsync(
        string ticketId,
        Guid userId,
        AddCommentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a comment to an external ticket via the provider API.
    /// </summary>
    /// <param name="ticketId">The external ticket ID (composite format: integrationId:externalTicketId)</param>
    /// <param name="userId">The user ID adding the comment</param>
    /// <param name="request">Comment content request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created comment DTO from the provider</returns>
    /// <exception cref="TicketNotFoundException">Thrown if integration not found</exception>
    /// <exception cref="UnauthorizedTicketAccessException">Thrown if user lacks workspace access</exception>
    /// <exception cref="InvalidOperationException">Thrown if provider call fails</exception>
    Task<CommentDto> AddCommentToExternalTicketAsync(
        string ticketId,
        Guid userId,
        AddCommentRequest request,
        CancellationToken cancellationToken = default);
}
