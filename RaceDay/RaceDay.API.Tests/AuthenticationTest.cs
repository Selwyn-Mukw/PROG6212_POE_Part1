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

        private const string TestJwtKey =
            "RaceDayIntegrationTestKey_12345678901234567890";

        public AuthenticationTests(
            RaceDayWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ProfileEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
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
            using var client = CreateAuthenticatedClient("Participant");

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
                "/api/Categories",
                request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_ReturnsForbidden()
        {
            // Arrange
            using var client = CreateAuthenticatedClient("Participant");

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
                "/api/Events",
                request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task CreateResult_AsParticipant_ReturnsForbidden()
        {
            // Arrange
            using var client = CreateAuthenticatedClient("Participant");

            using var request = new StringContent(
                """
                {
                    "enrolmentId": 1,
                    "finishTime": "00:25:30",
                    "finishPosition": 1
                }
                """,
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await client.PostAsync(
                "/api/Results",
                request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task UpdateResult_AsParticipant_ReturnsForbidden()
        {
            // Arrange
            using var client = CreateAuthenticatedClient("Participant");

            using var request = new StringContent(
                """
                {
                    "enrolmentId": 1,
                    "finishTime": "00:25:30",
                    "finishPosition": 1
                }
                """,
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await client.PutAsync(
                "/api/Results/1",
                request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task DeleteResult_AsParticipant_ReturnsForbidden()
        {
            // Arrange
            using var client = CreateAuthenticatedClient("Participant");

            // Act
            var response = await client.DeleteAsync(
                "/api/Results/1");

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task GetEventResults_AsParticipant_ReturnsForbidden()
        {
            // Arrange
            using var client = CreateAuthenticatedClient("Participant");

            // Act
            var response = await client.GetAsync(
                "/api/Results/event/1");

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task CreateEnrolment_AsOrganiser_ReturnsForbidden()
        {
            // Arrange
            using var client = CreateAuthenticatedClient("Organiser");

            using var request = new StringContent(
                """
        {
            "eventId": 1,
            "categoryId": 1
        }
        """,
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await client.PostAsync(
                "/api/Enrolments",
                request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task GetEventEnrolments_AsParticipant_ReturnsForbidden()
        {
            // Arrange
            using var client = CreateAuthenticatedClient("Participant");

            // Act
            var response = await client.GetAsync(
                "/api/Enrolments/event/1");

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task UpdateEnrolment_AsParticipant_ReturnsForbidden()
        {
            // Arrange
            using var client = CreateAuthenticatedClient("Participant");

            using var request = new StringContent(
                """
        {
            "enrolmentStatus": "Confirmed"
        }
        """,
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await client.PutAsync(
                "/api/Enrolments/1",
                request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task DeleteEnrolment_AsParticipant_ReturnsForbidden()
        {
            // Arrange
            using var client = CreateAuthenticatedClient("Participant");

            // Act
            var response = await client.DeleteAsync(
                "/api/Enrolments/1");

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        private HttpClient CreateAuthenticatedClient(string role)
        {
            var client = _factory.CreateClient();

            var token = CreateTestToken(role);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
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