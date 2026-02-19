using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Orchestra.Application.Common.Interfaces;
using Orchestra.Application.Tickets.Services;
using Xunit;

namespace Orchestra.Application.Tests.Tests.Tickets
{
    public class TicketPaginationServiceTests : IDisposable
    {
        private readonly ILogger<TicketPaginationService> _logger;
        private readonly TicketPaginationService _service;

        public TicketPaginationServiceTests()
        {
            _logger = Substitute.For<ILogger<TicketPaginationService>>();
            _service = new TicketPaginationService(_logger);
        }

        public void Dispose() { }

        [Fact]
        public void ParsePageToken_NullOrEmpty_ReturnsDefaultToken()
        {
            // Arrange/Act
            var result1 = _service.ParsePageToken(null);
            var result2 = _service.ParsePageToken("");
            var result3 = _service.ParsePageToken("   ");

            // Assert
            Assert.NotNull(result1);
            Assert.Equal("internal", result1.Phase);
            Assert.Equal(0, result1.InternalOffset);
            Assert.Null(result1.ExternalState);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
        }

        [Fact]
        public void ParsePageToken_InvalidBase64_LogsWarningAndReturnsDefault()
        {
            // Arrange
            var invalidToken = "not_base64!";

            // Act
            var result = _service.ParsePageToken(invalidToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("internal", result.Phase);
            _logger.Received().Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>()
            );
        }

        [Fact]
        public void ParsePageToken_ValidToken_ReturnsDeserializedToken()
        {
            // Arrange
            var token = new TicketPageToken
            {
                Phase = "external",
                InternalOffset = 42,
                ExternalState = new ExternalPaginationState
                {
                    CurrentProviderIndex = 2,
                    ProviderTokens = new Dictionary<string, string?> { { "id", "tok" } },
                    TotalExternalFetched = 5
                }
            };
            var json = JsonSerializer.Serialize(token);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            // Act
            var result = _service.ParsePageToken(base64);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("external", result.Phase);
            Assert.Equal(42, result.InternalOffset);
            Assert.NotNull(result.ExternalState);
            Assert.Equal(2, result.ExternalState.CurrentProviderIndex);
            Assert.Equal(5, result.ExternalState.TotalExternalFetched);
            Assert.True(result.ExternalState.ProviderTokens.ContainsKey("id"));
            Assert.Equal("tok", result.ExternalState.ProviderTokens["id"]);
        }

        [Fact]
        public void SerializePageToken_Null_ReturnsNull()
        {
            // Act
            var result = _service.SerializePageToken(null);
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void SerializePageToken_ValidToken_ReturnsBase64String()
        {
            // Arrange
            var token = new TicketPageToken
            {
                Phase = "external",
                InternalOffset = 7,
                ExternalState = new ExternalPaginationState
                {
                    CurrentProviderIndex = 1,
                    ProviderTokens = new Dictionary<string, string?> { { "x", "y" } },
                    TotalExternalFetched = 3
                }
            };

            // Act
            var result = _service.SerializePageToken(token);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result));
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(result));
            var deserialized = JsonSerializer.Deserialize<TicketPageToken>(decoded);
            Assert.NotNull(deserialized);
            Assert.Equal("external", deserialized.Phase);
            Assert.Equal(7, deserialized.InternalOffset);
            Assert.NotNull(deserialized.ExternalState);
            Assert.Equal(1, deserialized.ExternalState.CurrentProviderIndex);
            Assert.Equal(3, deserialized.ExternalState.TotalExternalFetched);
            Assert.True(deserialized.ExternalState.ProviderTokens.ContainsKey("x"));
            Assert.Equal("y", deserialized.ExternalState.ProviderTokens["x"]);
        }



        [Theory]
        [InlineData(0, 50)]
        [InlineData(-10, 50)]
        [InlineData(1, 1)]
        [InlineData(49, 49)]
        [InlineData(50, 50)]
        [InlineData(75, 75)]
        [InlineData(100, 100)]
        [InlineData(101, 100)]
        [InlineData(1000, 100)]
        public void NormalizePageSize_EnforcesBounds(int input, int expected)
        {
            // Act
            var result = _service.NormalizePageSize(input);
            // Assert
            Assert.Equal(expected, result);
        }
    }
}
