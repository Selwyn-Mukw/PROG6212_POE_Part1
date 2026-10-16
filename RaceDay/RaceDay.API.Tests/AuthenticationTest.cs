using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace RaceDay.API.Tests
{
    public class AuthenticationTests
        : IClassFixture<RaceDayWebApplicationFactory>
    {
        private readonly RaceDayWebApplicationFactory _factory;
        private readonly HttpClient _client;

        private const string TestJwtKey =
            "RaceDayIntegrationTestKey_12345678901234567890";

        public AuthenticationTests(
            RaceDayWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ProfileEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange: use a client without an authorization token.
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Profile/me");

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task CreateCategory_AsParticipant_ReturnsForbidden()
        {
            // Arrange
            var token = CreateTestToken("Participant");

            using var client = _factory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var request = new StringContent(
                """
                {
                    "eventId": 1,
                    "categoryName": "Integration Test Category",
                    "categoryType": "Running",
                    "minimumAge": 18,
                    "maximumAge": 40,
                    "categoryDistance": 5.00
                }
                """,
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await client.PostAsync(
                "/api/Categories", request);

            Console.WriteLine(
                $"Participant category test returned: {(int)response.StatusCode} {response.StatusCode}");

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_ReturnsForbidden()
        {
            // Arrange
            var token = CreateTestToken("Participant");

            using var client = _factory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var request = new StringContent(
                """
                {
                    "eventTypeId": 1,
                    "eventName": "Integration Test Event",
                    "description": "Test event",
                    "eventDate": "2027-06-15T09:00:00",
                    "location": "Johannesburg",
                    "distance": 5.00
                }
                """,
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await client.PostAsync(
                "/api/Events", request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        private static string CreateTestToken(string role)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(TestJwtKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(
                    ClaimTypes.Name,
                    "integration-test@example.com"),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: "RaceDayTestIssuer",
                audience: "RaceDayTestAudience",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}