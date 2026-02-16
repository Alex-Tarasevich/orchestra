using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Orchestra.Application.Common.Interfaces;
using Orchestra.Application.Tickets.DTOs;
using Orchestra.Application.Tickets.Services;
using Orchestra.Domain.Entities;
using Orchestra.Domain.Enums;
using Orchestra.Domain.Interfaces;
using Orchestra.Application.Tests.Builders;
using Xunit;

namespace Orchestra.Application.Tests.Tests.Tickets;

public class TicketExternalFetchingServiceTests
{
    private readonly ITicketDataAccess _ticketDataAccess = Substitute.For<ITicketDataAccess>();
    private readonly ITicketProviderFactory _ticketProviderFactory = Substitute.For<ITicketProviderFactory>();
    private readonly ITicketMappingService _ticketMappingService = Substitute.For<ITicketMappingService>();
    private readonly ILogger<TicketExternalFetchingService> _logger = Substitute.For<ILogger<TicketExternalFetchingService>>();
    private readonly TicketExternalFetchingService _sut;

    public TicketExternalFetchingServiceTests()
    {
        _sut = new TicketExternalFetchingService(
            _ticketDataAccess,
            _ticketProviderFactory,
            _ticketMappingService,
            _logger);
    }

    [Fact]
    public void CalculateProviderDistribution_EvenDistribution_ReturnsCorrectSlots()
    {
        // Arrange
        var integrations = new List<Integration>
        {
            new IntegrationBuilder().WithId(Guid.NewGuid()).Build(),
            new IntegrationBuilder().WithId(Guid.NewGuid()).Build(),
            new IntegrationBuilder().WithId(Guid.NewGuid()).Build()
        };
        int slots = 6;

        // Act
        var result = _sut.CalculateProviderDistribution(integrations, slots);

        // Assert
        Assert.All(result.Values, v => Assert.Equal(2, v));
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void CalculateProviderDistribution_WithRemainder_DistributesRemainder()
    {
        // Arrange
        var integrations = new List<Integration>
        {
            new IntegrationBuilder().WithId(Guid.NewGuid()).Build(),
            new IntegrationBuilder().WithId(Guid.NewGuid()).Build(),
            new IntegrationBuilder().WithId(Guid.NewGuid()).Build()
        };
        int slots = 8;

        // Act
        var result = _sut.CalculateProviderDistribution(integrations, slots);

        // Assert
        var slotCounts = result.Values.ToList();
        Assert.Equal(8, slotCounts.Sum());
        Assert.Equal(3, result.Count);
        // Two integrations should get 3 slots, one should get 2
        Assert.Equal(2, slotCounts.Count(v => v == 3));
        Assert.Equal(1, slotCounts.Count(v => v == 2));
    }

    [Fact]
    public void CalculateProviderDistribution_NoIntegrationsOrSlots_ReturnsEmpty()
    {
        // Arrange
        var integrations = new List<Integration>();
        int slots = 0;

        // Act
        var result = _sut.CalculateProviderDistribution(integrations, slots);

        // Assert
        Assert.Empty(result);
    }

    // Additional tests for FetchExternalTicketsAsync would require more extensive mocking
    // and are best implemented as integration or service tests with provider fakes.
}
