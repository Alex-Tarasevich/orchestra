namespace Orchestra.Domain.Enums;

/// <summary>
/// Defines the Confluence instance types supported by the system.
/// </summary>
public enum ConfluenceType
{
    /// <summary>
    /// Confluence Cloud instance using REST API v3.
    /// Accessed via https://[domain].atlassian.net
    /// </summary>
    Cloud = 0,

    /// <summary>
    /// Confluence Data Center or Server instance using REST API v2.
    /// Accessed via self-hosted or on-premise URL.
    /// </summary>
    OnPremise = 1
}
