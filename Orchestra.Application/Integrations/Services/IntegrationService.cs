using Orchestra.Application.Common.Exceptions;
using Orchestra.Application.Common.Interfaces;
using Orchestra.Application.Integrations.DTOs;
using Orchestra.Domain.Entities;
using Orchestra.Domain.Enums;
using Orchestra.Domain.Interfaces;

namespace Orchestra.Application.Integrations.Services;

public class IntegrationService : IIntegrationService
{
    private readonly IIntegrationDataAccess _integrationDataAccess;
    private readonly IWorkspaceAuthorizationService _workspaceAuthorizationService;
    private readonly ICredentialEncryptionService _credentialEncryptionService;

    public IntegrationService(
        IIntegrationDataAccess integrationDataAccess,
        IWorkspaceAuthorizationService workspaceAuthorizationService,
        ICredentialEncryptionService credentialEncryptionService)
    {
        _integrationDataAccess = integrationDataAccess;
        _workspaceAuthorizationService = workspaceAuthorizationService;
        _credentialEncryptionService = credentialEncryptionService;
    }

    public async Task<List<IntegrationDto>> GetWorkspaceIntegrationsAsync(
        Guid userId, 
        Guid workspaceId, 
        CancellationToken cancellationToken = default)
    {
        // 1. Verify user has access to workspace (authorization first!)
        await _workspaceAuthorizationService.ValidateMembershipAsync(
            userId, 
            workspaceId, 
            cancellationToken);

        // 2. Retrieve all active integrations for workspace
        var integrations = await _integrationDataAccess.GetByWorkspaceIdAsync(
            workspaceId, 
            cancellationToken);

        // 3. Map to DTOs (credentials are excluded)
        return integrations
            .Select(MapToDto)
            .ToList();
    }

    public async Task<IntegrationDto> CreateIntegrationAsync(
        Guid userId, 
        CreateIntegrationRequest request, 
        CancellationToken cancellationToken = default)
    {
        // 1. Verify user has access to workspace
        await _workspaceAuthorizationService.ValidateMembershipAsync(
            userId, 
            request.WorkspaceId, 
            cancellationToken);

        // 2. Parse and validate integration type
        if (!Enum.TryParse<IntegrationType>(request.Type, ignoreCase: true, out var integrationType))
        {
            throw new ArgumentException($"Invalid integration type: {request.Type}", nameof(request.Type));
        }

        // 3. Parse and validate provider type
        if (!Enum.TryParse<ProviderType>(request.Provider, ignoreCase: true, out var providerType))
        {
            var validProviders = string.Join(", ", Enum.GetNames<ProviderType>());
            throw new ArgumentException($"Invalid provider: {request.Provider}. Valid providers are: {validProviders}", nameof(request.Provider));
        }

        // 3b. Validate duplicate name
        var isDuplicate = await _integrationDataAccess.ExistsByNameInWorkspaceAsync(
            request.Name, 
            request.WorkspaceId, 
            cancellationToken: cancellationToken);

        if (isDuplicate)
        {
            throw new DuplicateIntegrationException(request.Name, request.WorkspaceId);
        }

        // 4. Encrypt API key
        var encryptedApiKey = string.IsNullOrEmpty(request.ApiKey) 
            ? null 
            : _credentialEncryptionService.Encrypt(request.ApiKey);

        // 5. Parse JiraType if provided and provider is JIRA
        Domain.Enums.JiraType? jiraType = null;
        if (providerType == ProviderType.JIRA && !string.IsNullOrEmpty(request.JiraType))
        {
            if (!Enum.TryParse<Domain.Enums.JiraType>(request.JiraType, ignoreCase: true, out var parsedJiraType))
            {
                var validJiraTypes = string.Join(", ", Enum.GetNames<Domain.Enums.JiraType>());
                throw new ArgumentException($"Invalid Jira type: {request.JiraType}. Valid types are: {validJiraTypes}", nameof(request.JiraType));
            }
            jiraType = parsedJiraType;
        }

        // 6. Create integration entity using domain factory
        var integration = Integration.Create(
            workspaceId: request.WorkspaceId,
            name: request.Name,
            type: integrationType,
            provider: providerType,
            url: request.Url,
            username: request.Username,
            encryptedApiKey: encryptedApiKey,
            filterQuery: request.FilterQuery,
            vectorize: request.Vectorize,
            jiraType: jiraType
        );

        // 7. Persist to database
        await _integrationDataAccess.AddAsync(integration, cancellationToken);

        // 8. Return DTO (without credentials)
        return MapToDto(integration);
    }

    public async Task<IntegrationDto> UpdateIntegrationAsync(
        Guid userId, 
        Guid integrationId, 
        UpdateIntegrationRequest request, 
        CancellationToken cancellationToken = default)
    {
        // 1. Retrieve the existing integration
        var integration = await _integrationDataAccess.GetByIdAsync(integrationId, cancellationToken);
        if (integration == null)
        {
            throw new IntegrationNotFoundException(integrationId);
        }

        // 2. Verify user has access to the integration's workspace
        await _workspaceAuthorizationService.ValidateMembershipAsync(
            userId, 
            integration.WorkspaceId, 
            cancellationToken);

        // 3. Parse and validate integration type
        if (!Enum.TryParse<IntegrationType>(request.Type, ignoreCase: true, out var integrationType))
        {
            throw new ArgumentException($"Invalid integration type: {request.Type}", nameof(request.Type));
        }

        // 4. Parse provider type if provided
        ProviderType? providerType = null;
        if (!string.IsNullOrEmpty(request.Provider))
        {
            if (!Enum.TryParse<ProviderType>(request.Provider, ignoreCase: true, out var parsedProvider))
            {
                var validProviders = string.Join(", ", Enum.GetNames<ProviderType>());
                throw new ArgumentException($"Invalid provider: {request.Provider}. Valid providers are: {validProviders}", nameof(request.Provider));
            }
            providerType = parsedProvider;
        }
        var isDuplicate = await _integrationDataAccess.ExistsByNameInWorkspaceAsync(
            request.Name, 
            integration.WorkspaceId, 
            excludeIntegrationId: integrationId,
            cancellationToken: cancellationToken);

        if (isDuplicate)
        {
            throw new DuplicateIntegrationException(request.Name, integration.WorkspaceId);
        }

        // 5. Handle API key: preserve existing if masked, otherwise encrypt new value
        string? encryptedApiKey = null;
        if (!string.IsNullOrEmpty(request.ApiKey) && request.ApiKey != "••••••••••••")
        {
            encryptedApiKey = _credentialEncryptionService.Encrypt(request.ApiKey);
        }

        // 6. Update the integration using domain method
        integration.Update(
            name: request.Name,
            provider: providerType,
            url: request.Url,
            username: request.Username,
            encryptedApiKey: encryptedApiKey,
            filterQuery: request.FilterQuery,
            vectorize: request.Vectorize
        );

        // 7. Persist changes to database
        await _integrationDataAccess.UpdateAsync(integration, cancellationToken);

        // 8. Return updated DTO (without credentials)
        return MapToDto(integration);
    }

    /// <summary>
    /// Soft deletes an integration by marking it as inactive.
    /// </summary>
    /// <param name="userId">The ID of the user performing the deletion.</param>
    /// <param name="integrationId">The ID of the integration to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="IntegrationNotFoundException">Thrown when the integration is not found.</exception>
    /// <exception cref="UnauthorizedWorkspaceAccessException">Thrown when the user is not a workspace member.</exception>
    public async Task DeleteIntegrationAsync(
        Guid userId, 
        Guid integrationId, 
        CancellationToken cancellationToken = default)
    {
        // 1. Load integration
        var integration = await _integrationDataAccess.GetByIdAsync(integrationId, cancellationToken)
            ?? throw new IntegrationNotFoundException(integrationId);

        // 2. Verify user has access to the workspace
        await _workspaceAuthorizationService.EnsureUserIsMemberAsync(
            userId, 
            integration.WorkspaceId, 
            cancellationToken);

        // 3. Soft delete integration
        integration.Deactivate();

        // 4. Persist changes
        await _integrationDataAccess.UpdateAsync(integration, cancellationToken);
    }

    private static IntegrationDto MapToDto(Integration integration)
    {
        return new IntegrationDto(
            Id: integration.Id.ToString(),
            WorkspaceId: integration.WorkspaceId.ToString(),
            Name: integration.Name,
            Type: integration.Type.ToString(),
            Icon: integration.Icon,
            Provider: integration.Provider.ToString(),
            Url: integration.Url,
            Username: integration.Username,
            Connected: integration.Connected,
            LastSync: FormatLastSync(integration.LastSyncAt),
            FilterQuery: integration.FilterQuery,
            Vectorize: integration.Vectorize
        );
    }

    private static string? FormatLastSync(DateTime? lastSyncAt)
    {
        if (!lastSyncAt.HasValue) return null;

        var timeSpan = DateTime.UtcNow - lastSyncAt.Value;
        
        if (timeSpan.TotalMinutes < 1) return "Just now";
        if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} min ago";
        if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
        if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays}d ago";
        
        return lastSyncAt.Value.ToString("MMM dd, yyyy");
    }
}
