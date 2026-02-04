using Orchestra.Application.Integrations.DTOs;

namespace Orchestra.Application.Common.Interfaces;

/// <summary>
/// Application service for integration management.
/// </summary>
public interface IIntegrationService
{
    /// <summary>
    /// Retrieves all active integrations for a workspace with authorization validation.
    /// </summary>
    /// <param name="userId">The requesting user's ID.</param>
    /// <param name="workspaceId">The workspace ID to filter integrations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of integration DTOs with credentials excluded.</returns>
    Task<List<IntegrationDto>> GetWorkspaceIntegrationsAsync(
        Guid userId, 
        Guid workspaceId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new integration for a workspace with authorization validation and credential encryption.
    /// </summary>
    /// <param name="userId">The requesting user's ID.</param>
    /// <param name="request">The create integration request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created integration DTO.</returns>
    Task<IntegrationDto> CreateIntegrationAsync(
        Guid userId, 
        CreateIntegrationRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing integration with authorization validation and conditional credential encryption.
    /// </summary>
    /// <param name="userId">The requesting user's ID.</param>
    /// <param name="integrationId">The ID of the integration to update.</param>
    /// <param name="request">The update integration request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated integration DTO.</returns>
    Task<IntegrationDto> UpdateIntegrationAsync(
        Guid userId, 
        Guid integrationId, 
        UpdateIntegrationRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes an integration by marking it as inactive.
    /// </summary>
    /// <param name="userId">The ID of the user performing the deletion.</param>
    /// <param name="integrationId">The ID of the integration to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="IntegrationNotFoundException">Thrown when the integration is not found.</exception>
    /// <exception cref="UnauthorizedWorkspaceAccessException">Thrown when the user is not a workspace member.</exception>
    Task DeleteIntegrationAsync(
        Guid userId, 
        Guid integrationId, 
        CancellationToken cancellationToken = default);
}
