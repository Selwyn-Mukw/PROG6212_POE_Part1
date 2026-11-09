using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay.Api.Services;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly AzureBlobStorageService _blobStorage;

        public ProfileController(
            SqlConnection connection,
            AzureBlobStorageService blobStorage)
        {
            _connection = connection;
            _blobStorage = blobStorage;
        }

        // GET: api/profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

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
                    return NotFound();

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
                    message = "A database error occurred."
                });
            }
            finally
            {
                if (_connection.State != System.Data.ConnectionState.Closed)
                    await _connection.CloseAsync();
            }
        }

        // PUT: api/profile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            [FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    UPDATE dbo.[USER]
                    SET FirstName = @FirstName,
                        LastName = @LastName,
                        PhoneNumber = @PhoneNumber
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

                command.Parameters.AddWithValue("@UserID", userId);

                int rowsUpdated = Convert.ToInt32(
                    await command.ExecuteScalarAsync());

                if (rowsUpdated == 0)
                    return NotFound();

                return Ok(new
                {
                    message = "Profile updated successfully."
                });
            }
            catch (SqlException)
            {
                return StatusCode(500, new
                {
                    message = "A database error occurred."
                });
            }
            finally
            {
                if (_connection.State != System.Data.ConnectionState.Closed)
                    await _connection.CloseAsync();
            }
        }

        // POST: api/profile/upload-picture
        [HttpPost("upload-picture")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadProfilePicture(
            IFormFile file)
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest("Please select an image.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Image must not exceed 5 MB.");

            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest(
                    "Only JPG, PNG and WEBP images are allowed.");

            string? newBlobName = null;
            string? oldBlobName = null;

            try
            {
                // STEP 1: Retrieve the existing picture filename

                await _connection.OpenAsync();

                const string selectSql = @"
                    SELECT ProfilePictureURL
                    FROM dbo.[USER]
                    WHERE UserID = @UserID;";

                using (var selectCommand =
                    new SqlCommand(selectSql, _connection))
                {
                    selectCommand.Parameters.AddWithValue(
                        "@UserID", userId);

                    var result =
                        await selectCommand.ExecuteScalarAsync();

                    if (result == null)
                        return NotFound("Profile not found.");

                    oldBlobName = result == DBNull.Value
                        ? null
                        : result.ToString();
                }

                await _connection.CloseAsync();

                // STEP 2: Upload the new picture to Azure

                newBlobName = await _blobStorage.UploadAsync(file);

                // STEP 3: Update SQL Server with the new filename

                await _connection.OpenAsync();

                const string updateSql = @"
                    UPDATE dbo.[USER]
                    SET ProfilePictureURL = @ProfilePictureURL
                    WHERE UserID = @UserID;";

                int rowsUpdated;

                using (var updateCommand =
                    new SqlCommand(updateSql, _connection))
                {
                    updateCommand.Parameters.AddWithValue(
                        "@ProfilePictureURL", newBlobName);

                    updateCommand.Parameters.AddWithValue(
                        "@UserID", userId);

                    rowsUpdated =
                        await updateCommand.ExecuteNonQueryAsync();
                }

                await _connection.CloseAsync();

                if (rowsUpdated == 0)
                {
                    await _blobStorage.DeleteAsync(newBlobName);

                    newBlobName = null;

                    return NotFound("Profile not found.");
                }

                // STEP 4: Delete the previous picture from Azure

                if (!string.IsNullOrWhiteSpace(oldBlobName))
                {
                    try
                    {
                        await _blobStorage.DeleteAsync(oldBlobName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"OLD PROFILE PICTURE DELETION FAILED: {ex}");
                    }
                }

                return Ok(new
                {
                    message = "Profile picture uploaded successfully.",
                    fileName = newBlobName
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"PROFILE PICTURE UPLOAD FAILED: {ex}");

                // Clean up the new picture if the operation fails

                if (!string.IsNullOrWhiteSpace(newBlobName))
                {
                    try
                    {
                        await _blobStorage.DeleteAsync(newBlobName);
                    }
                    catch (Exception cleanupException)
                    {
                        Console.WriteLine(
                            $"NEW PROFILE PICTURE CLEANUP FAILED: {cleanupException}");
                    }
                }

                return StatusCode(500, new
                {
                    message = "Unable to upload profile picture."
                });
            }
            finally
            {
                if (_connection.State != System.Data.ConnectionState.Closed)
                    await _connection.CloseAsync();
            }
        }

        // GET: api/profile/picture
        [HttpGet("picture")]
        public async Task<IActionResult> GetProfilePicture()
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    SELECT ProfilePictureURL
                    FROM dbo.[USER]
                    WHERE UserID = @UserID;";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters.AddWithValue("@UserID", userId);

                var result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                    return NotFound("No profile picture found.");

                string blobName = result.ToString()!;

                var picture =
                    await _blobStorage.DownloadAsync(blobName);

                return File(
                    picture.Content,
                    picture.ContentType);
            }
            catch (Azure.RequestFailedException ex)
                when (ex.Status == 404)
            {
                return NotFound(
                    "Profile picture not found in storage.");
            }
            catch (SqlException)
            {
                return StatusCode(
                    500,
                    "Database error retrieving profile picture.");
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    "Unable to retrieve profile picture.");
            }
            finally
            {
                if (_connection.State != System.Data.ConnectionState.Closed)
                    await _connection.CloseAsync();
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
    }
}