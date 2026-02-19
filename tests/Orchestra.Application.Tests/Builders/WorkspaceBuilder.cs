using Bogus;

namespace Orchestra.Application.Tests.Builders;

/// <summary>
/// Fluent builder for creating Workspace test entities with sensible defaults.
/// </summary>
public class WorkspaceBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = new Faker().Company.CompanyName();
    private Guid _ownerId = Guid.NewGuid();
    private bool _isActive = true;

    /// <summary>
    /// Sets the workspace ID.
    /// </summary>
    public WorkspaceBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the workspace name.
    /// </summary>
    public WorkspaceBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the owner ID.
    /// </summary>
    public WorkspaceBuilder WithOwnerId(Guid ownerId)
    {
        _ownerId = ownerId;
        return this;
    }

    /// <summary>
    /// Sets whether the workspace is active.
    /// </summary>
    public WorkspaceBuilder AsActive(bool active = true)
    {
        _isActive = active;
        return this;
    }

    /// <summary>
    /// Builds the Workspace entity.
    /// </summary>
    public Workspace Build()
    {
        return Workspace.Create(_name, _ownerId);
    }

    /// <summary>
    /// Creates an active workspace with typical configuration.
    /// </summary>
    public static Workspace ActiveWorkspace()
    {
        return new WorkspaceBuilder()
            .AsActive(true)
            .Build();
    }

    /// <summary>
    /// Creates an inactive workspace.
    /// </summary>
    public static Workspace InactiveWorkspace()
    {
        return new WorkspaceBuilder()
            .AsActive(false)
            .Build();
    }

    /// <summary>
    /// Creates a workspace with a specific owner.
    /// </summary>
    public static Workspace WorkspaceOwnedBy(Guid ownerId)
    {
        return new WorkspaceBuilder()
            .WithOwnerId(ownerId)
            .Build();
    }
}
