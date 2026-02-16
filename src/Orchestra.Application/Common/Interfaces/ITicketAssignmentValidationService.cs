namespace Orchestra.Application.Common.Interfaces;

/// <summary>
/// Service for validating ticket assignments (agents and workflows).
/// Ensures assigned entities belong to the same workspace as the ticket (FR-004).
/// </summary>
public interface ITicketAssignmentValidationService
{
    /// <summary>
    /// Validates agent exists and returns its workspace ID.
    /// Used before domain-level workspace consistency validation.
    /// </summary>
    /// <param name="agentId">The agent ID to validate (null is allowed for unassignment)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The agent's workspace ID if agentId is provided, null if agentId is null</returns>
    /// <exception cref="ArgumentException">Thrown when agentId is provided but agent not found</exception>
    Task<Guid?> ValidateAndGetAgentWorkspaceAsync(Guid? agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates workflow exists and returns its workspace ID.
    /// Used before domain-level workspace consistency validation.
    /// Note: Skipped until Workflow entity exists.
    /// </summary>
    /// <param name="workflowId">The workflow ID to validate (null is allowed for unassignment)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The workflow's workspace ID if workflowId is provided, null if workflowId is null</returns>
    /// <exception cref="ArgumentException">Thrown when workflowId is provided but workflow not found</exception>
    Task<Guid?> ValidateAndGetWorkflowWorkspaceAsync(Guid? workflowId, CancellationToken cancellationToken = default);
}
