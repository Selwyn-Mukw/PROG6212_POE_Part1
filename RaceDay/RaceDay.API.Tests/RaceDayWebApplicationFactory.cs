using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace RaceDay.API.Tests
{
    public class RaceDayWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        private const string TestJwtKey =
            "RaceDayIntegrationTestKey_12345678901234567890";

        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration(
                (context, configuration) =>
                {
                    var settings = new Dictionary<string, string?>
                    {
                        ["Jwt:Key"] = TestJwtKey,
                        ["Jwt:Issuer"] = "RaceDayTestIssuer",
                        ["Jwt:Audience"] = "RaceDayTestAudience"
                    };

                    configuration.AddInMemoryCollection(settings);
                });

            builder.ConfigureServices(services =>
            {
                services.PostConfigure<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.TokenValidationParameters.ValidIssuer =
                            "RaceDayTestIssuer";

                        options.TokenValidationParameters.ValidAudience =
                            "RaceDayTestAudience";

                        options.TokenValidationParameters.IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(TestJwtKey));
                    });
            });
        }
    }
}