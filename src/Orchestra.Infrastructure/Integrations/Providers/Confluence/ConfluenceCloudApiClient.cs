using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Web;
using Microsoft.Extensions.Logging;

namespace Orchestra.Infrastructure.Integrations.Providers.Confluence;

/// <summary>
/// Confluence Cloud API (v3) client implementation.
/// Handles API calls to Atlassian Cloud instances using REST API v3 endpoints.
/// </summary>
public class ConfluenceCloudApiClient : IConfluenceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ConfluenceCloudApiClient> _logger;

    public ConfluenceCloudApiClient(HttpClient httpClient, ILogger<ConfluenceCloudApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ConfluenceSearchResponse> SearchPagesAsync(
        string cql,
        int limit = 10,
        int start = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["cql"] = cql;
            query["limit"] = limit.ToString();
            query["start"] = start.ToString();

            var requestUrl = $"wiki/rest/api/content/search?{query}";
            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var searchResponse = System.Text.Json.JsonSerializer.Deserialize<ConfluenceSearchResponse>(
                content,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return searchResponse ?? new ConfluenceSearchResponse { Results = new List<ConfluencePage>() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search pages in Confluence Cloud: CQL={Cql}", cql);
            throw;
        }
    }

    public async Task<ConfluencePage?> GetPageAsync(
        string pageId,
        string expand = "body.atlas_doc_format,version,space",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["expand"] = expand;

            var requestUrl = $"wiki/rest/api/content/{pageId}?{query}";
            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Confluence page {PageId} not found in Cloud API", pageId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var page = System.Text.Json.JsonSerializer.Deserialize<ConfluencePage>(
                content,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get page {PageId} from Confluence Cloud", pageId);
            throw;
        }
    }

    public async Task<ConfluenceSpaceList> GetSpacesAsync(
        int limit = 25,
        int start = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["limit"] = limit.ToString();
            query["start"] = start.ToString();

            var response = await _httpClient.GetAsync($"wiki/rest/api/space?{query}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var spaces = System.Text.Json.JsonSerializer.Deserialize<ConfluenceSpaceList>(
                content,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return spaces ?? new ConfluenceSpaceList { Results = new List<ConfluenceSpace>() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get spaces from Confluence Cloud");
            throw;
        }
    }

    public async Task<ConfluenceSpace?> GetSpaceAsync(
        string spaceKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"wiki/rest/api/space/{spaceKey}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Confluence space {SpaceKey} not found in Cloud API", spaceKey);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var space = System.Text.Json.JsonSerializer.Deserialize<ConfluenceSpace>(
                content,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return space;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get space {SpaceKey} from Confluence Cloud", spaceKey);
            throw;
        }
    }

    public async Task<ConfluencePage?> CreatePageAsync(
        string spaceKey,
        string title,
        object body,
        string? parentPageId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var createRequest = new
            {
                type = "page",
                title = title,
                space = new { key = spaceKey },
                body = body,
                ancestors = parentPageId != null ? new[] { new { id = parentPageId } } : null
            };

            var response = await _httpClient.PostAsJsonAsync("wiki/rest/api/content", createRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var page = System.Text.Json.JsonSerializer.Deserialize<ConfluencePage>(
                content,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _logger.LogInformation("Created Confluence page {PageId} in space {SpaceKey}", page?.Id, spaceKey);
            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create page in Confluence Cloud space {SpaceKey}", spaceKey);
            throw;
        }
    }

    public async Task<ConfluencePage?> UpdatePageAsync(
        string pageId,
        string? title = null,
        object? body = null,
        int? currentVersion = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var newVersion = (currentVersion ?? 0) + 1;

            var updateRequest = new
            {
                type = "page",
                title = title,
                body = body,
                version = new { number = newVersion, message = "Updated via Orchestra" }
            };

            var response = await _httpClient.PutAsJsonAsync($"wiki/rest/api/content/{pageId}", updateRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var page = System.Text.Json.JsonSerializer.Deserialize<ConfluencePage>(
                content,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _logger.LogInformation("Updated Confluence page {PageId} to version {Version}", pageId, newVersion);
            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update page {PageId} in Confluence Cloud", pageId);
            throw;
        }
    }
}
