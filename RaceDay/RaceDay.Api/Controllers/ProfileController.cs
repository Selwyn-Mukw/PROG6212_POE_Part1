using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly SqlConnection _connection;

        public ProfileController(SqlConnection connection)
        {
            _connection = connection;
        }

        // GET: api/profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity in token."
                });
            }

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    SELECT UserID, FirstName, LastName, Email,
                           PhoneNumber, Role, ProfilePictureURL, CreatedAt
                    FROM dbo.[USER]
                    WHERE UserID = @UserID;";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters.AddWithValue("@UserID", userId);

                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        message = "Profile not found."
                    });
                }

                return Ok(new
                {
                    userId = reader.GetInt32(0),
                    firstName = reader.GetString(1),
                    lastName = reader.GetString(2),
                    email = reader.GetString(3),
                    phoneNumber = reader.IsDBNull(4)
                        ? null : reader.GetString(4),
                    role = reader.GetString(5),
                    profilePictureURL = reader.IsDBNull(6)
                        ? null : reader.GetString(6),
                    createdAt = reader.GetDateTime(7)
                });
            }
            catch (SqlException)
            {
                return StatusCode(500, new
                {
                    message = "A database error occurred while retrieving the profile."
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

        // PUT: api/profile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            [FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity in token."
                });
            }

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    UPDATE dbo.[USER]
                    SET FirstName = @FirstName,
                        LastName = @LastName,
                        PhoneNumber = @PhoneNumber,
                        ProfilePictureURL = @ProfilePictureURL
                    WHERE UserID = @UserID;

                    SELECT @@ROWCOUNT;";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters.AddWithValue(
                    "@FirstName", request.FirstName);

                command.Parameters.AddWithValue(
                    "@LastName", request.LastName);

                command.Parameters.AddWithValue(
                    "@PhoneNumber",
                    (object?)request.PhoneNumber ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@ProfilePictureURL",
                    (object?)request.ProfilePictureURL ?? DBNull.Value);

                command.Parameters.AddWithValue("@UserID", userId);

                int rowsUpdated = Convert.ToInt32(
                    await command.ExecuteScalarAsync());

                if (rowsUpdated == 0)
                {
                    return NotFound(new
                    {
                        message = "Profile not found."
                    });
                }

                return Ok(new
                {
                    message = "Profile updated successfully."
                });
            }
            catch (SqlException)
            {
                return StatusCode(500, new
                {
                    message = "A database error occurred while updating the profile."
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

    public class UpdateProfileRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.StringLength(30)]
        public string? PhoneNumber { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(500)]
        public string? ProfilePictureURL { get; set; }
    }
}