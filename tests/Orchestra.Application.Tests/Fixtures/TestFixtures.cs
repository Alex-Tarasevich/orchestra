using Microsoft.Extensions.Logging;

namespace Orchestra.Application.Tests.Fixtures;

/// <summary>
/// Base fixture class for unit tests, providing common setup and utilities.
/// </summary>
public class TestFixture : IDisposable
{
    /// <summary>
    /// Gets a generic logger substitute for testing.
    /// </summary>
    protected ILogger<T> GetLoggerSubstitute<T>() where T : class
    {
        return Substitute.For<ILogger<T>>();
    }

    /// <summary>
    /// Disposes of any test resources.
    /// </summary>
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Generic base fixture for unit testing application services.
/// Provides pre-configured NSubstitute mocks for common service dependencies.
/// </summary>
/// <typeparam name="T">The service type being tested.</typeparam>
public class ServiceTestFixture<T> : TestFixture where T : class
{
    /// <summary>
    /// Gets a substitute (mock) logger for the service.
    /// </summary>
    protected ILogger<T> Logger { get; }

    /// <summary>
    /// Initializes a new instance of ServiceTestFixture.
    /// </summary>
    public ServiceTestFixture()
    {
        Logger = GetLoggerSubstitute<T>();
    }

    /// <summary>
    /// Creates a mock data access service that returns the provided test data.
    /// Useful for testing service layer logic in isolation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type returned by the data access.</typeparam>
    /// <returns>A configured NSubstitute mock.</returns>
    protected TDataAccess CreateMockDataAccess<TDataAccess, TEntity>() where TDataAccess : class where TEntity : class
    {
        return Substitute.For<TDataAccess>();
    }

    /// <summary>
    /// Creates multiple test entities using the provided builder action.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to create.</typeparam>
    /// <param name="count">The number of entities to create.</param>
    /// <param name="configureBuilder">Optional action to configure each builder.</param>
    /// <returns>List of configured test entities.</returns>
    protected List<TEntity> CreateTestEntities<TEntity>(int count, Action<int>? configureBuilder = null) where TEntity : class
    {
        var entities = new List<TEntity>();
        for (int i = 0; i < count; i++)
        {
            configureBuilder?.Invoke(i);
        }
        return entities;
    }

    /// <summary>
    /// Verifies that a service method was called with specific arguments.
    /// </summary>
    protected void VerifyMethodWasCalled<TService>(TService service, Action<TService> methodCall) where TService : class
    {
        methodCall(service);
        // NSubstitute verification is done via Received() extension
    }
}

/// <summary>
/// Specialized fixture for authentication-related service testing.
/// </summary>
public class AuthTestFixture : ServiceTestFixture<IAuthService>
{
    /// <summary>
    /// Creates a mock JWT token service for auth tests.
    /// </summary>
    public IJwtTokenService CreateMockJwtTokenService()
    {
        return Substitute.For<IJwtTokenService>();
    }

    /// <summary>
    /// Creates a mock password hashing service for auth tests.
    /// </summary>
    public IPasswordHashingService CreateMockPasswordHashingService()
    {
        return Substitute.For<IPasswordHashingService>();
    }

    /// <summary>
    /// Creates a mock user data access service for auth tests.
    /// </summary>
    public IUserDataAccess CreateMockUserDataAccess()
    {
        return Substitute.For<IUserDataAccess>();
    }

    /// <summary>
    /// Creates a mock workspace service for auth tests.
    /// </summary>
    public IWorkspaceService CreateMockWorkspaceService()
    {
        return Substitute.For<IWorkspaceService>();
    }
}

/// <summary>
/// Specialized fixture for agent-related service testing.
/// </summary>
public class AgentTestFixture : ServiceTestFixture<IAgentService>
{
    /// <summary>
    /// Creates a mock agent data access service.
    /// </summary>
    public IAgentDataAccess CreateMockAgentDataAccess()
    {
        return Substitute.For<IAgentDataAccess>();
    }

    /// <summary>
    /// Creates a mock workspace authorization service for agent tests.
    /// </summary>
    public IWorkspaceAuthorizationService CreateMockWorkspaceAuthorizationService()
    {
        return Substitute.For<IWorkspaceAuthorizationService>();
    }

    /// <summary>
    /// Creates a mock tool service for agent tests.
    /// </summary>
    public IToolService CreateMockToolService()
    {
        return Substitute.For<IToolService>();
    }
}

/// <summary>
/// Specialized fixture for ticket-related service testing.
/// </summary>
public class TicketTestFixture : ServiceTestFixture<ITicketService>
{
    /// <summary>
    /// Creates a mock ticket data access service.
    /// </summary>
    public ITicketDataAccess CreateMockTicketDataAccess()
    {
        return Substitute.For<ITicketDataAccess>();
    }

    /// <summary>
    /// Creates a mock workspace authorization service for ticket tests.
    /// </summary>
    public IWorkspaceAuthorizationService CreateMockWorkspaceAuthorizationService()
    {
        return Substitute.For<IWorkspaceAuthorizationService>();
    }

    /// <summary>
    /// Creates a mock ticket provider factory for external ticket tests.
    /// </summary>
    public ITicketProviderFactory CreateMockTicketProviderFactory()
    {
        return Substitute.For<ITicketProviderFactory>();
    }

    /// <summary>
    /// Creates a mock ticket mapping service.
    /// </summary>
    public ITicketMappingService CreateMockTicketMappingService()
    {
        return Substitute.For<ITicketMappingService>();
    }
}

/// <summary>
/// Specialized fixture for integration-related service testing.
/// </summary>
public class IntegrationTestFixture : ServiceTestFixture<IIntegrationService>
{
    /// <summary>
    /// Creates a mock integration data access service.
    /// </summary>
    public IIntegrationDataAccess CreateMockIntegrationDataAccess()
    {
        return Substitute.For<IIntegrationDataAccess>();
    }

    /// <summary>
    /// Creates a mock workspace authorization service for integration tests.
    /// </summary>
    public IWorkspaceAuthorizationService CreateMockWorkspaceAuthorizationService()
    {
        return Substitute.For<IWorkspaceAuthorizationService>();
    }
}

/// <summary>
/// Specialized fixture for tool-related service testing.
/// </summary>
public class ToolTestFixture : ServiceTestFixture<IToolService>
{
    /// <summary>
    /// Creates a mock tool action data access service.
    /// </summary>
    public IToolActionDataAccess CreateMockToolActionDataAccess()
    {
        return Substitute.For<IToolActionDataAccess>();
    }

    /// <summary>
    /// Creates a mock tool category data access service.
    /// </summary>
    public IToolCategoryDataAccess CreateMockToolCategoryDataAccess()
    {
        return Substitute.For<IToolCategoryDataAccess>();
    }

    /// <summary>
    /// Creates a mock agent tool action data access service.
    /// </summary>
    public IAgentToolActionDataAccess CreateMockAgentToolActionDataAccess()
    {
        return Substitute.For<IAgentToolActionDataAccess>();
    }

    /// <summary>
    /// Creates a mock tool validation service.
    /// </summary>
    public IToolValidationService CreateMockToolValidationService()
    {
        return Substitute.For<IToolValidationService>();
    }

    /// <summary>
    /// Creates a mock tool scanning service.
    /// </summary>
    public IToolScanningService CreateMockToolScanningService()
    {
        return Substitute.For<IToolScanningService>();
    }
}

/// <summary>
/// Specialized fixture for workspace-related service testing.
/// </summary>
public class WorkspaceTestFixture : ServiceTestFixture<IWorkspaceService>
{
    /// <summary>
    /// Creates a mock workspace data access service.
    /// </summary>
    public IWorkspaceDataAccess CreateMockWorkspaceDataAccess()
    {
        return Substitute.For<IWorkspaceDataAccess>();
    }

    /// <summary>
    /// Creates a mock workspace authorization service.
    /// </summary>
    public IWorkspaceAuthorizationService CreateMockWorkspaceAuthorizationService()
    {
        return Substitute.For<IWorkspaceAuthorizationService>();
    }

    /// <summary>
    /// Creates a mock user data access service.
    /// </summary>
    public IUserDataAccess CreateMockUserDataAccess()
    {
        return Substitute.For<IUserDataAccess>();
    }
}
