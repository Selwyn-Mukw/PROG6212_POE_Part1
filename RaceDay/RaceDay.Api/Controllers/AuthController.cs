using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RaceDay.contracts.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly IConfiguration _configuration;

        public AuthController(
            SqlConnection connection,
            IConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("RaceDay Participant acccess confirmed!");
        }

        [Authorize(Roles = "Organiser")]
        [HttpGet("organiser-test")]
        public IActionResult OrganiserTest()
        {
            return Ok("RaceDay Organiser access confirmed!");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _connection.OpenAsync();

                // Check whether the email already exists.
                const string checkEmailSql = @"
                    SELECT COUNT(1)
                    FROM dbo.[USER]
                    WHERE Email = @Email;";

                using var checkCommand = new SqlCommand(
                    checkEmailSql, _connection);

                checkCommand.Parameters.AddWithValue(
                    "@Email", request.Email);

                int existingUsers = Convert.ToInt32(
                    await checkCommand.ExecuteScalarAsync());

                if (existingUsers > 0)
                {
                    return Conflict(new
                    {
                        message =
                            "This email address is already registered."
                    });
                }

                // Hash the password before storing it.
                var passwordHasher = new PasswordHasher<string>();

                string passwordHash = passwordHasher.HashPassword(
                    request.Email,
                    request.Password);

                // Insert the new user into the database.
                const string insertSql = @"
                    INSERT INTO dbo.[USER]
                        (FirstName, LastName, Email,
                         PhoneNumber, PasswordHash, Role)
                    VALUES
                        (@FirstName, @LastName, @Email,
                         @PhoneNumber, @PasswordHash, @Role);";

                using var insertCommand = new SqlCommand(
                    insertSql, _connection);

                insertCommand.Parameters.AddWithValue(
                    "@FirstName", request.FirstName);

                insertCommand.Parameters.AddWithValue(
                    "@LastName", request.LastName);

                insertCommand.Parameters.AddWithValue(
                    "@Email", request.Email);

                insertCommand.Parameters.AddWithValue(
                    "@PhoneNumber", request.PhoneNumber);

                insertCommand.Parameters.AddWithValue(
                    "@PasswordHash", passwordHash);

                insertCommand.Parameters.AddWithValue(
                    "@Role", "Participant");

                await insertCommand.ExecuteNonQueryAsync();

                return Ok(new
                {
                    message = "Registration completed successfully.",
                    firstName = request.FirstName,
                    lastName = request.LastName,
                    email = request.Email,
                    role = "Participant"
                });
            }
            catch (SqlException)
            {
                return StatusCode(500, new
                {
                    message =
                        "A database error occurred during registration."
                });
            }
            finally
            {
                if (_connection.State !=
                    System.Data.ConnectionState.Closed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    SELECT UserID, FirstName, LastName,
                           Email, PasswordHash, Role
                    FROM dbo.[USER]
                    WHERE Email = @Email;";

                using var command = new SqlCommand(
                    sql, _connection);

                command.Parameters.AddWithValue(
                    "@Email", request.Email);

                int userId;
                string firstName;
                string lastName;
                string email;
                string passwordHash;
                string role;

                using (var reader =
                    await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        return Unauthorized(new
                        {
                            message = "Invalid email or password."
                        });
                    }

                    userId = reader.GetInt32(0);
                    firstName = reader.GetString(1);
                    lastName = reader.GetString(2);
                    email = reader.GetString(3);
                    passwordHash = reader.GetString(4);
                    role = reader.GetString(5);
                }

                

                // Verify the submitted password.
                var passwordHasher =
                    new PasswordHasher<string>();

                var result =
                    passwordHasher.VerifyHashedPassword(
                        email,
                        passwordHash,
                        request.Password);

                if (result ==
                    PasswordVerificationResult.Failed)
                {
                    return Unauthorized(new
                    {
                        message = "Invalid email or password."
                    });
                }

                // Read JWT settings from configuration.
                var jwtKey = _configuration["Jwt:Key"];
                var jwtIssuer = _configuration["Jwt:Issuer"];
                var jwtAudience = _configuration["Jwt:Audience"];

                if (string.IsNullOrWhiteSpace(jwtKey) ||
                    string.IsNullOrWhiteSpace(jwtIssuer) ||
                    string.IsNullOrWhiteSpace(jwtAudience))
                {
                    return StatusCode(500, new
                    {
                        message =
                            "JWT configuration is incomplete."
                    });
                }

                // Create claims for the authenticated user.
                var claims = new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        userId.ToString()),

                    new Claim(
                        ClaimTypes.Name,
                        email),

                    new Claim(
                        ClaimTypes.Email,
                        email),

                    new Claim(
                        ClaimTypes.Role,
                        role),

                    new Claim(
                        "firstName",
                        firstName),

                    new Claim(
                        "lastName",
                        lastName)
                };

                // Sign the token using the secret key.
                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey));

                var credentials = new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);

                var expiryMinutes =
                    _configuration.GetValue<int>(
                        "Jwt:ExpiryMinutes", 60);

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(
                        expiryMinutes),
                    signingCredentials: credentials);

                string tokenString =
                    new JwtSecurityTokenHandler()
                        .WriteToken(token);

                // Return the token to the client.
                return Ok(new
                {
                    message = "Login successful.",
                    userId,
                    firstName,
                    lastName,
                    email,
                    role,
                    token = tokenString
                });
            }
            catch (SqlException)
            {
                return StatusCode(500, new
                {
                    message =
                        "A database error occurred during login."
                });
            }
            finally
            {
                if (_connection.State !=
                    System.Data.ConnectionState.Closed)
                {
                    await _connection.CloseAsync();
                }
            }
        }
    }
}