using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using RaceDay.contracts.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly AzureBlobStorageService _blobStorage;

        public EventsController(
            SqlConnection connection,
            AzureBlobStorageService blobStorage)
        {
            _connection = connection;
            _blobStorage = blobStorage;
        }

        // GET: api/Events/types
        // Retrieves all available event types.
        [HttpGet("types")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventTypes()
        {
            var eventTypes = new List<object>();

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    SELECT EventTypeID, TypeName
                    FROM dbo.[EVENT TYPE]
                    ORDER BY EventTypeID;";

                using var command = new SqlCommand(sql, _connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    eventTypes.Add(new
                    {
                        EventTypeId = reader.GetInt32(0),
                        EventTypeName = reader.GetString(1)
                    });
                }

                return Ok(eventTypes);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "Unable to retrieve event types."
                    });
            }
            finally
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        // GET: api/Events
        // Retrieves all events.
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = new List<EventDto>();

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    SELECT EventID, EventName, EventDate, Location, BannerImageURL
                    FROM dbo.EVENT
                    ORDER BY EventDate;";

                using var command = new SqlCommand(sql, _connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    events.Add(new EventDto
                    {
                        EventId = reader.GetInt32(0),
                        EventName = reader.GetString(1),
                        EventDate = reader.GetDateTime(2),
                        Location = reader.GetString(3),
                        BannerImageURL = reader.IsDBNull(4)
                            ? null
                            : reader.GetString(4)
                    });
                }

                return Ok(events);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while retrieving events."
                    });
            }
            finally
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        // GET: api/Events/{id}
        // Retrieves one event using its ID, including all editable details.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Event ID must be greater than zero."
                });
            }

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    SELECT EventID, EventTypeID, EventName, Description,
                           EventDate, Location, Distance, BannerImageURL
                    FROM dbo.EVENT
                    WHERE EventID = @EventID;";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters.Add("@EventID", SqlDbType.Int).Value = id;

                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        message = $"Event with ID {id} was not found."
                    });
                }

                var eventDetails = new
                {
                    EventId = reader.GetInt32(0),
                    EventTypeId = reader.GetInt32(1),
                    EventName = reader.GetString(2),
                    Description = reader.IsDBNull(3)
                        ? null
                        : reader.GetString(3),
                    EventDate = reader.GetDateTime(4),
                    Location = reader.GetString(5),
                    Distance = reader.GetDecimal(6),
                    BannerImageURL = reader.IsDBNull(7)
                        ? null
                        : reader.GetString(7)
                };

                return Ok(eventDetails);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while retrieving the event."
                    });
            }
            finally
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        // GET: api/Events/mine
        // Retrieves events created by the authenticated organiser.
        [HttpGet("mine")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetMyEvents()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int organiserId) ||
                organiserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Unable to identify the authenticated organiser."
                });
            }

            var events = new List<object>();

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
            SELECT EventID, EventName, EventDate, Location , BannerImageURL
            FROM dbo.EVENT
            WHERE OrganiserID = @OrganiserID
            ORDER BY EventDate;";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters.Add("@OrganiserID", SqlDbType.Int)
                    .Value = organiserId;

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    events.Add(new
                    {
                        EventId = reader.GetInt32(0),
                        EventName = reader.GetString(1),
                        EventDate = reader.GetDateTime(2),
                        Location = reader.GetString(3),
                        BannerImageURL = reader.IsDBNull(4)
                            ? null
                            : reader.GetString(4)
                    });
                }

                return Ok(events);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while retrieving your events."
                    });
            }
            finally
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        // POST: api/Events
        // Creates an event. Only authenticated organisers may do this.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent(
            [FromBody] CreateEventDto request)
        {
            // Get the organiser's ID from the authenticated user's JWT.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")
                ?? User.FindFirst("userId");

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int organiserId) ||
                organiserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Unable to identify the authenticated organiser."
                });
            }

            try
            {
                await _connection.OpenAsync();

                // Check that the selected event type exists.
                const string eventTypeSql = @"
                    SELECT COUNT(1)
                    FROM dbo.[EVENT TYPE]
                    WHERE EventTypeID = @EventTypeID;";

                using var eventTypeCommand =
                    new SqlCommand(eventTypeSql, _connection);

                eventTypeCommand.Parameters
                    .Add("@EventTypeID", SqlDbType.Int)
                    .Value = request.EventTypeId;

                var eventTypeExists =
                    Convert.ToInt32(
                        await eventTypeCommand.ExecuteScalarAsync()) > 0;

                if (!eventTypeExists)
                {
                    return BadRequest(new
                    {
                        message = "The specified event type does not exist."
                    });
                }

                // Insert the new event and return its saved details.
                const string sql = @"
                    INSERT INTO dbo.EVENT
                    (
                        OrganiserID,
                        EventTypeID,
                        EventName,
                        Description,
                        EventDate,
                        Location,
                        Distance
                    )
                    OUTPUT
                        INSERTED.EventID,
                        INSERTED.OrganiserID,
                        INSERTED.EventTypeID,
                        INSERTED.EventName,
                        INSERTED.Description,
                        INSERTED.EventDate,
                        INSERTED.Location,
                        INSERTED.Distance
                    VALUES
                    (
                        @OrganiserID,
                        @EventTypeID,
                        @EventName,
                        @Description,
                        @EventDate,
                        @Location,
                        @Distance
                    );";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters.Add("@OrganiserID", SqlDbType.Int)
                    .Value = organiserId;

                command.Parameters.Add("@EventTypeID", SqlDbType.Int)
                    .Value = request.EventTypeId;

                command.Parameters.Add("@EventName", SqlDbType.VarChar, 100)
                    .Value = request.EventName;

                command.Parameters.Add("@Description", SqlDbType.VarChar, 500)
                    .Value = (object?)request.Description ?? DBNull.Value;

                command.Parameters.Add("@EventDate", SqlDbType.DateTime2)
                    .Value = request.EventDate;

                command.Parameters.Add("@Location", SqlDbType.VarChar, 150)
                    .Value = request.Location;

                var distanceParameter =
                    command.Parameters.Add("@Distance", SqlDbType.Decimal);

                distanceParameter.Precision = 6;
                distanceParameter.Scale = 2;
                distanceParameter.Value = request.Distance;

                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new
                        {
                            message = "The event could not be created."
                        });
                }

                var createdEvent = new
                {
                    EventId = reader.GetInt32(0),
                    OrganiserId = reader.GetInt32(1),
                    EventTypeId = reader.GetInt32(2),
                    EventName = reader.GetString(3),
                    Description = reader.IsDBNull(4)
                        ? null
                        : reader.GetString(4),
                    EventDate = reader.GetDateTime(5),
                    Location = reader.GetString(6),
                    Distance = reader.GetDecimal(7)
                };

                return StatusCode(
                    StatusCodes.Status201Created,
                    createdEvent);
            }
            catch (SqlException ex) when (
                ex.Number == 2601 || ex.Number == 2627)
            {
                // These SQL Server errors indicate a duplicate key.
                return Conflict(new
                {
                    message = "An event with the same unique value already exists."
                });
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while creating the event."
                    });
            }
            finally
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        // POST: api/Events/{id}/banner
        // Uploads or replaces an event banner in Azure Blob Storage.
        [HttpPost("{id:int}/banner")]
        [Authorize(Roles = "Organiser")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadEventBanner(
            int id,
            IFormFile? file)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Event ID must be greater than zero."
                });
            }

            // Identify the authenticated organiser.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")
                ?? User.FindFirst("userId");

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int organiserId) ||
                organiserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Unable to identify the authenticated organiser."
                });
            }

            // Validate the uploaded file.
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please select an image to upload."
                });
            }

            const long maxFileSize = 5 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                return BadRequest(new
                {
                    message = "The banner image cannot exceed 5 MB."
                });
            }

            string[] allowedContentTypes =
            {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

            if (!allowedContentTypes.Contains(
                file.ContentType,
                StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Only JPG, PNG and WebP images are allowed."
                });
            }

            string? oldBannerName = null;
            string? newBannerName = null;
            bool databaseUpdated = false;

            try
            {
                // Check that the event belongs to this organiser.
                await _connection.OpenAsync();

                const string checkEventSql = @"
            SELECT BannerImageURL
            FROM dbo.EVENT
            WHERE EventID = @EventID
              AND OrganiserID = @OrganiserID;";

                using (var command = new SqlCommand(
                    checkEventSql, _connection))
                {
                    command.Parameters.Add("@EventID", SqlDbType.Int)
                        .Value = id;

                    command.Parameters.Add("@OrganiserID", SqlDbType.Int)
                        .Value = organiserId;

                    using var reader = await command.ExecuteReaderAsync();

                    if (!await reader.ReadAsync())
                    {
                        return NotFound(new
                        {
                            message = "The event was not found or you are not its organiser."
                        });
                    }

                    oldBannerName = reader.IsDBNull(0)
                        ? null
                        : reader.GetString(0);
                }

                await _connection.CloseAsync();

                // Upload the new banner to Azure.
                newBannerName =
                    await _blobStorage.UploadEventBannerAsync(file);

                // Save the new banner filename in the database.
                await _connection.OpenAsync();

                const string updateSql = @"
            UPDATE dbo.EVENT
            SET BannerImageURL = @BannerImageURL
            WHERE EventID = @EventID
              AND OrganiserID = @OrganiserID;";

                using (var updateCommand = new SqlCommand(
                    updateSql, _connection))
                {
                    updateCommand.Parameters
                        .Add("@BannerImageURL", SqlDbType.VarChar, 255)
                        .Value = newBannerName;

                    updateCommand.Parameters
                        .Add("@EventID", SqlDbType.Int)
                        .Value = id;

                    updateCommand.Parameters
                        .Add("@OrganiserID", SqlDbType.Int)
                        .Value = organiserId;

                    int rowsAffected =
                        await updateCommand.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new
                        {
                            message = "The event could not be updated."
                        });
                    }

                    databaseUpdated = true;
                }

                await _connection.CloseAsync();

                // Delete the previous banner after successful replacement.
                if (!string.IsNullOrWhiteSpace(oldBannerName))
                {
                    try
                    {
                        await _blobStorage.DeleteEventBannerAsync(
                            oldBannerName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Previous event banner could not be deleted: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    message = "Event banner uploaded successfully.",
                    eventId = id,
                    bannerImageURL = newBannerName
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Event banner upload failed: {ex.Message}");

                // Remove the newly uploaded file if the database update failed.
                if (!databaseUpdated &&
                    !string.IsNullOrWhiteSpace(newBannerName))
                {
                    try
                    {
                        await _blobStorage.DeleteEventBannerAsync(
                            newBannerName);
                    }
                    catch (Exception cleanupException)
                    {
                        Console.WriteLine(
                            $"New banner cleanup failed: {cleanupException.Message}");
                    }
                }

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "An error occurred while uploading the event banner."
                    });
            }
            finally
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        // GET: api/Events/{id}/banner
        // Retrieves an event banner from Azure Blob Storage.
        [HttpGet("{id:int}/banner")]
        public async Task<IActionResult> GetEventBanner(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Event ID must be greater than zero."
                });
            }

            string? bannerName = null;

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
            SELECT BannerImageURL
            FROM dbo.EVENT
            WHERE EventID = @EventID;";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters.Add("@EventID", SqlDbType.Int).Value = id;

                var result = await command.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    bannerName = result.ToString();
                }
            }
            catch (SqlException)
            {
                return StatusCode(500, new
                {
                    message = "A database error occurred while retrieving the event banner."
                });
            }
            finally
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    await _connection.CloseAsync();
                }
            }

            if (string.IsNullOrWhiteSpace(bannerName))
            {
                return NotFound(new
                {
                    message = "No banner image was found for this event."
                });
            }

            try
            {
                var image = await _blobStorage.DownloadEventBannerAsync(bannerName);

                return File(image.Content, image.ContentType);
            }
            catch (Exception)
            {
                return NotFound(new
                {
                    message = "The event banner could not be found in storage."
                });
            }
        }

        // PUT: api/Events/{id}
        // Updates an existing event.
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            [FromBody] UpdateEventDto request)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Event ID must be greater than zero."
                });
            }

            // Identify the authenticated organiser.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")
                ?? User.FindFirst("userId");

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int organiserId) ||
                organiserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Unable to identify the authenticated organiser."
                });
            }

            try
            {
                await _connection.OpenAsync();

                // Check whether this event belongs to the authenticated organiser.
                const string checkEventSql = @"
            SELECT COUNT(1)
            FROM dbo.EVENT
            WHERE EventID = @EventID
              AND OrganiserID = @OrganiserID;";

                using var checkEventCommand =
                    new SqlCommand(checkEventSql, _connection);

                checkEventCommand.Parameters.Add("@EventID", SqlDbType.Int)
                    .Value = id;

                checkEventCommand.Parameters.Add("@OrganiserID", SqlDbType.Int)
                    .Value = organiserId;

                var eventExists = Convert.ToInt32(
                    await checkEventCommand.ExecuteScalarAsync()) > 0;

                if (!eventExists)
                {
                    return NotFound(new
                    {
                        message = "The event was not found or you are not its organiser."
                    });
                }

                // Confirm that the supplied event type exists.
                const string eventTypeSql = @"
            SELECT COUNT(1)
            FROM dbo.[EVENT TYPE]
            WHERE EventTypeID = @EventTypeID;";

                using var eventTypeCommand =
                    new SqlCommand(eventTypeSql, _connection);

                eventTypeCommand.Parameters.Add("@EventTypeID", SqlDbType.Int)
                    .Value = request.EventTypeId;

                var eventTypeExists = Convert.ToInt32(
                    await eventTypeCommand.ExecuteScalarAsync()) > 0;

                if (!eventTypeExists)
                {
                    return BadRequest(new
                    {
                        message = "The specified event type does not exist."
                    });
                }

                // Update the event and return the updated details.
                const string updateSql = @"
            UPDATE dbo.EVENT
            SET
                EventTypeID = @EventTypeID,
                EventName = @EventName,
                Description = @Description,
                EventDate = @EventDate,
                Location = @Location,
                Distance = @Distance
            OUTPUT
                INSERTED.EventID,
                INSERTED.OrganiserID,
                INSERTED.EventTypeID,
                INSERTED.EventName,
                INSERTED.Description,
                INSERTED.EventDate,
                INSERTED.Location,
                INSERTED.Distance
            WHERE EventID = @EventID
              AND OrganiserID = @OrganiserID;";

                using var command = new SqlCommand(updateSql, _connection);

                command.Parameters.Add("@EventID", SqlDbType.Int)
                    .Value = id;

                command.Parameters.Add("@OrganiserID", SqlDbType.Int)
                    .Value = organiserId;

                command.Parameters.Add("@EventTypeID", SqlDbType.Int)
                    .Value = request.EventTypeId;

                command.Parameters.Add("@EventName", SqlDbType.VarChar, 100)
                    .Value = request.EventName;

                command.Parameters.Add("@Description", SqlDbType.VarChar, 500)
                    .Value = (object?)request.Description ?? DBNull.Value;

                command.Parameters.Add("@EventDate", SqlDbType.DateTime2)
                    .Value = request.EventDate;

                command.Parameters.Add("@Location", SqlDbType.VarChar, 150)
                    .Value = request.Location;

                var distanceParameter =
                    command.Parameters.Add("@Distance", SqlDbType.Decimal);

                distanceParameter.Precision = 6;
                distanceParameter.Scale = 2;
                distanceParameter.Value = request.Distance;

                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        message = "The event could not be found or updated."
                    });
                }

                var updatedEvent = new
                {
                    EventId = reader.GetInt32(0),
                    OrganiserId = reader.GetInt32(1),
                    EventTypeId = reader.GetInt32(2),
                    EventName = reader.GetString(3),
                    Description = reader.IsDBNull(4)
                        ? null
                        : reader.GetString(4),
                    EventDate = reader.GetDateTime(5),
                    Location = reader.GetString(6),
                    Distance = reader.GetDecimal(7)
                };

                return Ok(updatedEvent);
            }
            catch (SqlException ex) when (
                ex.Number == 2601 || ex.Number == 2627)
            {
                return Conflict(new
                {
                    message = "An event with the same unique value already exists."
                });
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while updating the event."
                    });
            }
            finally
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    await _connection.CloseAsync();
                }
            }
        }
        // DELETE: api/Events/{id}
        // Deletes an event owned by the authenticated organiser.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Event ID must be greater than zero."
                });
            }

            // Identify the authenticated organiser.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")
                ?? User.FindFirst("userId");

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int organiserId) ||
                organiserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Unable to identify the authenticated organiser."
                });
            }

            try
            {
                await _connection.OpenAsync();

                // Check whether the event belongs to this organiser.
                const string checkEventSql = @"
            SELECT COUNT(1)
            FROM dbo.EVENT
            WHERE EventID = @EventID
              AND OrganiserID = @OrganiserID;";

                using var checkEventCommand =
                    new SqlCommand(checkEventSql, _connection);

                checkEventCommand.Parameters
                    .Add("@EventID", SqlDbType.Int).Value = id;

                checkEventCommand.Parameters
                    .Add("@OrganiserID", SqlDbType.Int).Value = organiserId;

                var eventExists = Convert.ToInt32(
                    await checkEventCommand.ExecuteScalarAsync()) > 0;

                if (!eventExists)
                {
                    return NotFound(new
                    {
                        message = "The event was not found or you are not its organiser."
                    });
                }

                // Delete only the event owned by the authenticated organiser.
                const string deleteSql = @"
            DELETE FROM dbo.EVENT
            WHERE EventID = @EventID
              AND OrganiserID = @OrganiserID;";

                using var deleteCommand =
                    new SqlCommand(deleteSql, _connection);

                deleteCommand.Parameters
                    .Add("@EventID", SqlDbType.Int).Value = id;

                deleteCommand.Parameters
                    .Add("@OrganiserID", SqlDbType.Int).Value = organiserId;

                var rowsAffected = await deleteCommand.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return NotFound(new
                    {
                        message = "The event could not be found or deleted."
                    });
                }

                return Ok(new
                {
                    message = $"Event with ID {id} was deleted successfully."
                });
            }
            catch (SqlException ex) when (
                ex.Number == 547)
            {
                // SQL Server error 547 commonly indicates a foreign-key conflict.
                return Conflict(new
                {
                    message = "This event cannot be deleted because related records exist. Resolve the related records before deleting the event."
                });
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while deleting the event."
                    });
            }
            finally
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    await _connection.CloseAsync();
                }
            }
        }
    }
}