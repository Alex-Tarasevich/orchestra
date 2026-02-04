using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Net;
using System.Web;
using Microsoft.Extensions.Logging;
using Orchestra.Application.Common.Interfaces;
using Orchestra.Application.Integrations.Services;
using Orchestra.Application.Tickets.DTOs;
using Orchestra.Domain.Entities;
using Orchestra.Domain.Interfaces;
using Orchestra.Infrastructure.Integrations.Providers.Jira.Models;

namespace Orchestra.Infrastructure.Integrations.Providers.Jira;

public class JiraTicketProvider : ITicketProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICredentialEncryptionService _credentialEncryptionService;
    private readonly ILogger<JiraTicketProvider> _logger;
    private readonly IAdfConversionService _adfConversionService;

    public JiraTicketProvider(
        IHttpClientFactory httpClientFactory,
        ICredentialEncryptionService credentialEncryptionService,
        ILogger<JiraTicketProvider> logger,
        IAdfConversionService adfConversionService)
    {
        _httpClientFactory = httpClientFactory;
        _credentialEncryptionService = credentialEncryptionService;
        _logger = logger;
        _adfConversionService = adfConversionService;
    }

    private HttpClient GetHttpClient(Integration integration)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            if (string.IsNullOrEmpty(integration.Url))
            {
                throw new ArgumentException("Integration URL is required for Jira API calls.", nameof(integration.Url));
            }
            
            if (string.IsNullOrEmpty(integration.EncryptedApiKey))
            {
                throw new ArgumentException("Integration encrypted API key is required for Jira API calls.", nameof(integration.EncryptedApiKey));
            }
            
            client.BaseAddress = new Uri(integration.Url);
            
            // Decrypt API key (format: "email:apiToken")
            var apiKey = _credentialEncryptionService.Decrypt(integration.EncryptedApiKey);
            
            // Set Basic Auth header
            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{integration.Username}:{apiKey}"));
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Basic", authValue);
            
            return client;
        }
        catch (UriFormatException ex)
        {
            _logger.LogError(ex, "Invalid URL format for Jira integration '{IntegrationName}': '{Url}'", 
                integration.Name, integration.Url);
            throw new ArgumentException("Invalid integration URL format.", nameof(integration.Url), ex);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is CryptographicException)
        {
            _logger.LogError(ex, "Failed to configure HttpClient for Jira integration '{IntegrationName}'", 
                integration.Name);
            throw;
        }
    }

    public async Task<(List<ExternalTicketDto> Tickets, bool IsLast, string? NextPageToken)> 
        FetchTicketsAsync(
            Integration integration,
            int startAt = 0,
            int maxResults = 50,
            string? pageToken = null,
            CancellationToken cancellationToken = default)
    {
        var client = GetHttpClient(integration);
        var filter = integration.FilterQuery;
        var jql = !string.IsNullOrWhiteSpace(filter) 
            ? $"{filter} ORDER BY priority DESC, updated DESC" 
            : "ORDER BY priority DESC, updated DESC";
        
        try
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["jql"] = jql;
            query["fields"] = "key,status,priority,summary,description,comment,created,updated";
            query["startAt"] = startAt.ToString();
            query["maxResults"] = maxResults.ToString();
            
            if (!string.IsNullOrWhiteSpace(pageToken))
            {
                query["nextPageToken"] = pageToken;
            }
            
            var requestUrl = $"rest/api/3/search/jql?{query}";
            
            var response = await client.GetAsync(requestUrl, cancellationToken);
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();
            var searchResponse = JsonSerializer.Deserialize<JiraSearchResponse>(
                content, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (searchResponse == null)
            {
                _logger.LogWarning(
                    "Jira search returned null response for integration {IntegrationName} (ID: {IntegrationId}), " +
                    "URL={RequestUrl}",
                    integration.Name,
                    integration.Id,
                    requestUrl);
                return (new List<ExternalTicketDto>(), true, null);
            }
            
            _logger.LogDebug("Fetched {TicketCount} tickets, IsLast={IsLast}",
                searchResponse.Tickets?.Count ?? 0, searchResponse.IsLast);
            
            var tickets = (await Task.WhenAll(searchResponse.Tickets?.Select(async jiraTicket => 
                await MapJiraTicketToDtoAsync(jiraTicket, integration, cancellationToken)) ?? Array.Empty<Task<ExternalTicketDto>>())).ToList();
            
            return (tickets, searchResponse.IsLast, searchResponse.NextPageToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to fetch tickets from Jira integration {IntegrationName} (ID: {IntegrationId}): " +
                "JQL={Jql}, StartAt={StartAt}, MaxResults={MaxResults}, NextPageToken={NextPageToken}",
                integration.Name,
                integration.Id,
                jql,
                startAt,
                maxResults,
                pageToken ?? "(none)");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error fetching tickets from Jira integration {IntegrationName} (ID: {IntegrationId}): " +
                "JQL={Jql}, StartAt={StartAt}, MaxResults={MaxResults}, NextPageToken={NextPageToken}",
                integration.Name,
                integration.Id,
                jql,
                startAt,
                maxResults,
                pageToken ?? "(none)");
            throw;
        }
    }

    public async Task<ExternalTicketDto?> GetTicketByIdAsync(
        Integration integration,
        string externalTicketId,
        CancellationToken cancellationToken = default)
    {
        var client = GetHttpClient(integration);
        var fields = "key,status,priority,summary,description,comment,created,updated";
        
        try
        {
            var response = await client.GetAsync(
                $"/rest/api/3/issue/{externalTicketId}?fields={fields}", 
                cancellationToken);
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Jira ticket {TicketId} not found in integration {IntegrationId}",
                    externalTicketId, integration.Id);
                return null;
            }
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var jiraTicket = JsonSerializer.Deserialize<JiraTicket>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return jiraTicket == null ? null : await MapJiraTicketToDtoAsync(jiraTicket, integration, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch ticket {TicketId} from Jira integration {IntegrationId}",
                externalTicketId, integration.Id);
            throw;
        }
    }

    public async Task<CommentDto> AddCommentAsync(
        Integration integration,
        string externalTicketId,
        string content,
        string author,
        CancellationToken cancellationToken = default)
    {
        var client = GetHttpClient(integration);
        
        // Convert markdown to ADF using the conversion service
        var adfBody = await _adfConversionService.ConvertMarkdownToAdfAsync(content, cancellationToken);
        
        var requestBody = new
        {
            body = adfBody
        };
        
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");
        
        try
        {
            var response = await client.PostAsync(
                $"/rest/api/3/issue/{externalTicketId}/comment",
                jsonContent,
                cancellationToken);
            
            response.EnsureSuccessStatusCode();
            
            return new CommentDto(
                Guid.NewGuid().ToString(),
                author,
                content
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to add comment to Jira ticket {TicketId}",
                externalTicketId);
            throw;
        }
    }

    public async Task<ExternalTicketCreationResult> CreateIssueAsync(
        Integration integration,
        string summary,
        string description,
        string issueTypeName,
        CancellationToken cancellationToken = default)
    {
        var client = GetHttpClient(integration);
        
        try
        {
            _logger.LogInformation(
                "Creating JIRA issue in integration {IntegrationId}: Summary='{Summary}', IssueType='{IssueType}'",
                integration.Id,
                summary,
                issueTypeName);
            
            // Step 1: Get project ID from filter query
            var projectId = await GetProjectIdFromFilterQueryAsync(integration, cancellationToken);
            
            // Step 2: Resolve issue type name to ID
            var issueTypeId = await GetIssueTypeIdAsync(integration, issueTypeName, cancellationToken);
            
            // Step 3: Convert markdown description to ADF
            var adfDescription = await _adfConversionService.ConvertMarkdownToAdfAsync(description, cancellationToken);
            
            // Step 4: Build create issue request
            var request = new CreateIssueRequest
            {
                Fields = new CreateIssueFields
                {
                    Summary = summary,
                    Description = adfDescription,
                    Issuetype = new IssueTypeField { Id = issueTypeId },
                    Project = new ProjectField { Id = projectId }
                }
            };
            
            _logger.LogDebug(
                "Creating JIRA issue in project {ProjectId} with type {IssueTypeId} for integration {IntegrationId}", 
                projectId, 
                issueTypeId, 
                integration.Id);
            
            var response = await client.PostAsJsonAsync(
                "/rest/api/3/issue", 
                request, 
                cancellationToken);
            
            // Handle specific HTTP status codes
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogError(
                    "Failed to authenticate with JIRA for integration {IntegrationId}",
                    integration.Id);
                throw new InvalidOperationException(
                    "Failed to authenticate with JIRA. Please verify the API key.");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _logger.LogError(
                    "Access forbidden when creating issue in integration {IntegrationId}",
                    integration.Id);
                throw new InvalidOperationException(
                    "You do not have permission to create issues in JIRA.");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "JIRA API validation error for integration {IntegrationId}: {ErrorContent}",
                    integration.Id,
                    errorContent);
                throw new ArgumentException($"JIRA validation failed: {errorContent}");
            }
            
            if ((int)response.StatusCode >= 500)
            {
                _logger.LogError(
                    "JIRA server error {StatusCode} for integration {IntegrationId}",
                    response.StatusCode,
                    integration.Id);
                throw new HttpRequestException(
                    "JIRA server error occurred. Please try again later.");
            }
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var createResponse = JsonSerializer.Deserialize<CreateIssueResponse>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (createResponse == null || string.IsNullOrEmpty(createResponse.Key))
            {
                _logger.LogError("JIRA returned null or empty issue key for integration {IntegrationId}", integration.Id);
                throw new InvalidOperationException("Failed to create JIRA issue: No issue key returned.");
            }
            
            var baseUrl = integration.Url?.TrimEnd('/') ?? string.Empty;
            var issueUrl = $"{baseUrl}/browse/{createResponse.Key}";
            
            _logger.LogInformation(
                "Successfully created JIRA issue {IssueKey} in integration {IntegrationId}",
                createResponse.Key,
                integration.Id);
            
            return new ExternalTicketCreationResult(
                IssueKey: createResponse.Key,
                IssueUrl: issueUrl,
                IssueId: createResponse.Id
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to create JIRA issue in integration {IntegrationId}", integration.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating JIRA issue in integration {IntegrationId}", integration.Id);
            throw;
        }
    }

    private async Task<string> GetProjectIdFromFilterQueryAsync(
        Integration integration,
        CancellationToken cancellationToken = default)
    {
        var client = GetHttpClient(integration);
        
        // Build JQL query from FilterQuery (or use default if empty)
        var jql = string.IsNullOrEmpty(integration.FilterQuery) 
            ? "ORDER BY updated DESC" 
            : integration.FilterQuery;
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["jql"] = jql;
        query["fields"] = "project";

        // Call JIRA Search API to get project from first result
        var encodedJql = HttpUtility.UrlEncode(jql);
        var requestUrl = $"/rest/api/3/search/jql?{query}";
        
        _logger.LogDebug(
            "Fetching project ID from JIRA integration {IntegrationId} using JQL: {Jql}", 
            integration.Id, 
            jql);
        
        var response = await client.GetAsync(requestUrl, cancellationToken);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Failed to authenticate with JIRA for integration {IntegrationId}", integration.Id);
            throw new InvalidOperationException("Failed to authenticate with JIRA. Please verify the API key.");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Invalid JQL query in FilterQuery for integration {IntegrationId}: {ErrorContent}", integration.Id, errorContent);
            throw new ArgumentException("Invalid JQL query in integration FilterQuery. Please check the JQL syntax.");
        }
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var searchResponse = JsonSerializer.Deserialize<JiraSearchResponse>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        var projectId = searchResponse?.Tickets?.FirstOrDefault()?.Fields?.Project?.Id;
        
        if (string.IsNullOrEmpty(projectId))
        {
            _logger.LogError("No project found in FilterQuery results for integration {IntegrationId}", integration.Id);
            throw new InvalidOperationException(
                $"No project found in FilterQuery results for integration {integration.Id}. " +
                $"Ensure FilterQuery returns at least one issue.");
        }
        
        _logger.LogDebug("Resolved project ID {ProjectId} for integration {IntegrationId}", projectId, integration.Id);
        return projectId;
    }

    private async Task<string> GetIssueTypeIdAsync(
        Integration integration,
        string issueTypeName,
        CancellationToken cancellationToken = default)
    {
        var client = GetHttpClient(integration);
        
        _logger.LogDebug(
            "Fetching issue type ID for '{IssueTypeName}' from JIRA integration {IntegrationId}", 
            issueTypeName, 
            integration.Id);
        
        var response = await client.GetAsync("/rest/api/3/issuetype", cancellationToken);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Failed to authenticate with JIRA for integration {IntegrationId}", integration.Id);
            throw new InvalidOperationException("Failed to authenticate with JIRA. Please verify the API key.");
        }
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var issueTypes = JsonSerializer.Deserialize<List<IssueType>>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        var issueType = issueTypes?.FirstOrDefault(it => 
            string.Equals(it.Name, issueTypeName, StringComparison.OrdinalIgnoreCase));
        
        if (issueType == null)
        {
            _logger.LogError("Issue type '{IssueTypeName}' not found in JIRA integration {IntegrationId}", issueTypeName, integration.Id);
            throw new ArgumentException(
                $"Issue type '{issueTypeName}' not found in JIRA integration {integration.Id}. " +
                $"Available types: {string.Join(", ", issueTypes?.Select(it => it.Name) ?? new List<string>())}");
        }
        
        _logger.LogDebug(
            "Resolved issue type '{IssueTypeName}' to ID {IssueTypeId} for integration {IntegrationId}", 
            issueTypeName, 
            issueType.Id, 
            integration.Id);
        
        return issueType.Id;
    }

    /// <summary>
    /// Maps a Jira ticket to ExternalTicketDto with ADF-to-Markdown conversion for description and comments.
    /// </summary>
    /// <param name="jiraTicket">The Jira ticket from API response.</param>
    /// <param name="integration">The integration configuration.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>ExternalTicketDto with Markdown-formatted content.</returns>
    /// <exception cref="HttpRequestException">Thrown when ADF conversion service is unavailable.</exception>
    /// <exception cref="InvalidOperationException">Thrown when ADF conversion fails after retries.</exception>
    private async Task<ExternalTicketDto> MapJiraTicketToDtoAsync(JiraTicket jiraTicket, Integration integration, CancellationToken cancellationToken = default)
    {
        var statusName = jiraTicket.Fields?.Status?.Name ?? "Unknown";
        var priorityName = jiraTicket.Fields?.Priority?.Name ?? "Medium";
        
        var priorityValue = MapPriorityToValue(priorityName);
        
        var comments = await ConvertCommentsAsync(jiraTicket.Fields?.Comment?.Comments, cancellationToken);
        
        // Build external URL directly from integration base URL and ticket key
        var baseUrl = integration.Url?.TrimEnd('/') ?? string.Empty;
        var externalUrl = !string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(jiraTicket.Key)
            ? $"{baseUrl}/browse/{jiraTicket.Key}"
            : string.Empty;
        
        return new ExternalTicketDto(
            IntegrationId: integration.Id,
            ExternalTicketId: jiraTicket.Key ?? "UNKNOWN",
            Title: jiraTicket.Fields?.Summary ?? "Untitled",
            Description: await ExtractDescriptionTextAsync(jiraTicket.Fields?.Description, cancellationToken),
            StatusName: statusName,
            StatusColor: GetStatusColor(statusName),
            PriorityName: priorityName,
            PriorityColor: GetPriorityColor(priorityName),
            PriorityValue: priorityValue,
            ExternalUrl: externalUrl,
            Comments: comments
        );
    }

    /// <summary>
    /// Batch-converts all Jira comments from ADF (Atlassian Document Format) to Markdown in a single HTTP call.
    /// </summary>
    /// <param name="jiraComments">List of Jira comments with ADF body content.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>List of CommentDto objects with Markdown-formatted content.</returns>
    /// <exception cref="HttpRequestException">Thrown when ADF conversion service is unavailable.</exception>
    /// <exception cref="InvalidOperationException">Thrown when batch ADF conversion fails after retries.</exception>
    private async Task<List<CommentDto>> ConvertCommentsAsync(IEnumerable<JiraComment>? jiraComments, CancellationToken cancellationToken = default)
    {
        if (jiraComments == null || !jiraComments.Any())
        {
            return new List<CommentDto>();
        }
        
        try
        {
            // Collect all ADF structures from comments
            var adfElements = new List<JsonElement>();
            foreach (var comment in jiraComments)
            {
                var json = JsonSerializer.Serialize(comment.Body);
                var adfElement = JsonSerializer.Deserialize<JsonElement>(json);
                adfElements.Add(adfElement);
            }
            
            // Batch convert all comments in a single HTTP call
            var markdownResults = await _adfConversionService.ConvertAdfBatchToMarkdownAsync(adfElements, cancellationToken);
            
            _logger.LogDebug("Successfully batch-converted {Count} comments from ADF to Markdown", 
                markdownResults.Count);
            
            // Map results to CommentDto
            var comments = new List<CommentDto>();
            for (int i = 0; i < jiraComments.Count(); i++)
            {
                var jiraComment = jiraComments.ElementAt(i);
                var markdown = i < markdownResults.Count ? markdownResults[i] : string.Empty;
                
                comments.Add(new CommentDto(
                    jiraComment.Id ?? Guid.NewGuid().ToString(),
                    jiraComment.Author?.DisplayName ?? "Unknown",
                    markdown ?? string.Empty
                ));
            }
            
            return comments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to batch-convert {Count} comments from ADF to Markdown. Sync operation will fail.", 
                jiraComments.Count());
            throw;
        }
    }

    /// <summary>
    /// Converts Jira ticket description from ADF (Atlassian Document Format) to Markdown.
    /// </summary>
    /// <param name="description">The ADF-formatted description JSON element from Jira API.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Markdown-formatted description, or empty string if description is null/empty.</returns>
    /// <exception cref="HttpRequestException">Thrown when ADF conversion service is unavailable.</exception>
    /// <exception cref="InvalidOperationException">Thrown when ADF conversion fails after retries.</exception>
    private async Task<string> ExtractDescriptionTextAsync(JsonElement? description, CancellationToken cancellationToken = default)
    {
        if (!description.HasValue || 
            description.Value.ValueKind == JsonValueKind.Undefined || 
            description.Value.ValueKind == JsonValueKind.Null)
        {
            return string.Empty;
        }
        
        try
        {
            var markdown = await _adfConversionService.ConvertAdfToMarkdownAsync(description.Value, cancellationToken);
            _logger.LogDebug("Successfully converted description ADF to Markdown: {MarkdownLength} characters", 
                markdown?.Length ?? 0);
            return markdown ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert description ADF to Markdown. Sync operation will fail.");
            throw;
        }
    }

    private int MapPriorityToValue(string priorityName)
    {
        return priorityName.ToLowerInvariant() switch
        {
            "highest" or "critical" or "blocker" => 4,
            "high" => 3,
            "medium" or "normal" => 2,
            "low" or "lowest" or "trivial" => 1,
            _ => 2 // Default to medium
        };
    }

    private string GetStatusColor(string statusName)
    {
        var lowerStatus = statusName.ToLowerInvariant();
        
        if (lowerStatus.Contains("done") || lowerStatus.Contains("complete") || lowerStatus.Contains("closed"))
            return "bg-emerald-500/20 text-emerald-400";
        
        if (lowerStatus.Contains("progress") || lowerStatus.Contains("review"))
            return "bg-yellow-500/20 text-yellow-400";
        
        if (lowerStatus.Contains("todo") || lowerStatus.Contains("to do"))
            return "bg-purple-500/20 text-purple-400";
        
        return "bg-blue-500/20 text-blue-400"; // Default for new/open
    }

    private string GetPriorityColor(string priorityName)
    {
        var lowerPriority = priorityName.ToLowerInvariant();
        
        if (lowerPriority.Contains("highest") || lowerPriority.Contains("critical") || lowerPriority.Contains("blocker"))
            return "bg-red-500/10 text-red-400 border border-red-500/20";
        
        if (lowerPriority.Contains("high"))
            return "bg-orange-500/10 text-orange-400 border border-orange-500/20";
        
        if (lowerPriority.Contains("low") || lowerPriority.Contains("lowest") || lowerPriority.Contains("trivial"))
            return "bg-slate-500/10 text-slate-400 border border-slate-500/20";
        
        return "bg-blue-500/10 text-blue-400 border border-blue-500/20"; // Default for medium
    }
}