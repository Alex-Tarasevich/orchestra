using Orchestra.Application.Common.Interfaces;
using Orchestra.Application.Tickets.DTOs;
using Microsoft.Extensions.Logging;

namespace Orchestra.Application.Tickets.Services;

/// <summary>
/// Orchestrator service for ticket operations.
/// Delegates to specialized services:
/// - ITicketQueryService for read operations
/// - ITicketCommandService for write operations
/// - IExternalTicketFetchingService for multi-provider pagination
/// - ITicketMaterializationService for external ticket materialization
/// </summary>
public class TicketService : ITicketService
{
    private readonly ITicketQueryService _queryService;
    private readonly ITicketCommandService _commandService;
    private readonly ITicketCommentService _commentService;
    private readonly ILogger<TicketService> _logger;
    private readonly ITicketEnrichmentService _enrichmentService;

    public TicketService(
        ITicketQueryService queryService,
        ITicketCommandService commandService,
        ITicketCommentService commentService,
        ITicketEnrichmentService enrichmentService,
        ILogger<TicketService> logger)
    {
        _queryService = queryService;
        _commandService = commandService;
        _commentService = commentService;
        _enrichmentService = enrichmentService;
        _logger = logger;
    }

    public async Task<TicketDto> CreateTicketAsync(Guid userId, CreateTicketRequest request, CancellationToken cancellationToken = default)
        => await _commandService.CreateTicketAsync(userId, request, cancellationToken);

    public async Task<PaginatedTicketsResponse> GetTicketsAsync(
        Guid workspaceId,
        Guid userId,
        string? pageToken = null,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
        => await _queryService.GetTicketsAsync(workspaceId, userId, pageToken, pageSize, cancellationToken);

    public async Task<TicketDto> GetTicketByIdAsync(
        string ticketId,
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _queryService.GetTicketByIdAsync(ticketId, userId, cancellationToken);

    public async Task<List<TicketStatusDto>> GetAllStatusesAsync(CancellationToken cancellationToken = default)
        => await _queryService.GetAllStatusesAsync(cancellationToken);

    public async Task<List<TicketPriorityDto>> GetAllPrioritiesAsync(CancellationToken cancellationToken = default)
        => await _queryService.GetAllPrioritiesAsync(cancellationToken);

    public async Task<TicketDto> UpdateTicketAsync(
        string ticketId,
        Guid userId,
        UpdateTicketRequest request,
        CancellationToken cancellationToken = default)
        => await _commandService.UpdateTicketAsync(ticketId, userId, request, cancellationToken);

    public async Task<TicketDto> ConvertToExternalAsync(
        string ticketId,
        Guid userId,
        Guid integrationId,
        string issueTypeName,
        CancellationToken cancellationToken = default)
        => await _commandService.ConvertToExternalAsync(ticketId, userId, integrationId, issueTypeName, cancellationToken);

    public async Task DeleteTicketAsync(
        string ticketId,
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _commandService.DeleteTicketAsync(ticketId, userId, cancellationToken);

    public async Task<CommentDto> AddCommentAsync(
        string ticketId,
        Guid userId,
        AddCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _commentService.AddCommentAsync(ticketId, userId, request, cancellationToken);
    }

    public async Task<TicketDto> GenerateSummaryAsync(
        string ticketId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // 1. Fetch the ticket (handles both internal and external tickets)
        var ticketDto = await _queryService.GetTicketByIdAsync(ticketId, userId, cancellationToken);

        // 2. Build content string for summarization
        var content = _enrichmentService.BuildSummaryContent(ticketDto);

        // 3. Generate summary using enrichment service
        string summary = await _enrichmentService.GenerateSummaryAsync(content, cancellationToken);

        // 4. Return ticket DTO with summary populated
        return ticketDto with { Summary = summary };
    }
}