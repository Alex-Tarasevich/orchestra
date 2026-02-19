namespace Orchestra.Infrastructure.Integrations.Providers.Confluence;

/// <summary>
/// Abstract API client interface for Confluence operations.
/// Implementations support both Cloud (v3) and On-Premise (v2) API versions.
/// </summary>
public interface IConfluenceApiClient
{
    /// <summary>
    /// Searches for pages using CQL (Confluence Query Language).
    /// </summary>
    /// <param name="cql">The CQL query string.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="start">Pagination offset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search response containing pages and pagination info.</returns>
    Task<ConfluenceSearchResponse> SearchPagesAsync(
        string cql,
        int limit = 10,
        int start = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single page by ID.
    /// </summary>
    /// <param name="pageId">The page ID.</param>
    /// <param name="expand">Comma-separated list of properties to expand.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Confluence page, or null if not found.</returns>
    Task<ConfluencePage?> GetPageAsync(
        string pageId,
        string expand = "body.atlas_doc_format,version,space",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all spaces.
    /// </summary>
    /// <param name="limit">Maximum number of spaces to return.</param>
    /// <param name="start">Pagination offset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of spaces.</returns>
    Task<ConfluenceSpaceList> GetSpacesAsync(
        int limit = 25,
        int start = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets space by key.
    /// </summary>
    /// <param name="spaceKey">The space key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The space, or null if not found.</returns>
    Task<ConfluenceSpace?> GetSpaceAsync(
        string spaceKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new Confluence page.
    /// </summary>
    /// <param name="spaceKey">The space key where the page will be created.</param>
    /// <param name="title">The page title.</param>
    /// <param name="body">The page body content object (ADF for Cloud, appropriate format for On-Premise).</param>
    /// <param name="parentPageId">Optional parent page ID for creating a child page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created page.</returns>
    Task<ConfluencePage?> CreatePageAsync(
        string spaceKey,
        string title,
        object body,
        string? parentPageId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing Confluence page.
    /// </summary>
    /// <param name="pageId">The page ID to update.</param>
    /// <param name="title">New page title (optional).</param>
    /// <param name="body">New page body content object (optional).</param>
    /// <param name="currentVersion">The current version number of the page (required for optimistic locking).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated page.</returns>
    Task<ConfluencePage?> UpdatePageAsync(
        string pageId,
        string? title = null,
        object? body = null,
        int? currentVersion = null,
        CancellationToken cancellationToken = default);
}
