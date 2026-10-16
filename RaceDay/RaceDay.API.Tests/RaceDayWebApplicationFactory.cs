using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace RaceDay.API.Tests
{
    public class RaceDayWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration(
                (context, configuration) =>
                {
                    var settings = new Dictionary<string, string?>
                    {
                        ["Jwt:Key"] =
                            "RaceDayIntegrationTestKey_12345678901234567890",
                        ["Jwt:Issuer"] = "RaceDayTestIssuer",
                        ["Jwt:Audience"] = "RaceDayTestAudience"
                    };

                    configuration.AddInMemoryCollection(settings);
                });

            builder.ConfigureServices(services =>
            {
                // Keep the test JWT settings consistent with the test token.
                services.PostConfigure<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.TokenValidationParameters.ValidIssuer =
                            "RaceDayTestIssuer";

                        options.TokenValidationParameters.ValidAudience =
                            "RaceDayTestAudience";

                        options.TokenValidationParameters.IssuerSigningKey =
                            new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                                System.Text.Encoding.UTF8.GetBytes(
                                    "RaceDayIntegrationTestKey_12345678901234567890"));
                    });
            });
        }
    }
}