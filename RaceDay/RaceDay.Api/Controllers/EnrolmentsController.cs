using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using RaceDay.contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly SqlConnection _connection;

        public EnrolmentsController(SqlConnection connection)
        {
            _connection = connection;
        }

        // GET: api/Enrolments
        // Retrieves enrolments belonging only to the authenticated participant.
        [HttpGet]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetEnrolments()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")
                ?? User.FindFirst("userId");

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int participantId) ||
                participantId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Unable to identify the authenticated participant."
                });
            }

            var enrolments = new List<EnrolmentDto>();

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    SELECT
                        EnrolmentID,
                        ParticipantID,
                        EventID,
                        CategoryID,
                        EnrolmentDate,
                        EnrolmentStatus
                    FROM dbo.ENROLMENT
                    WHERE ParticipantID = @ParticipantID
                    ORDER BY EnrolmentDate DESC;";

                using var command = new SqlCommand(sql, _connection);
                command.Parameters.Add("@ParticipantID", SqlDbType.Int).Value = participantId;

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    enrolments.Add(new EnrolmentDto
                    {
                        EnrolmentId = reader.GetInt32(0),
                        ParticipantId = reader.GetInt32(1),
                        EventId = reader.GetInt32(2),
                        CategoryId = reader.GetInt32(3),
                        EnrolmentDate = reader.GetDateTime(4),
                        EnrolmentStatus = reader.GetString(5)
                    });
                }

                return Ok(enrolments);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while retrieving your enrolments."
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

        // GET: api/Enrolments/mine
        // Retrieves enrolments belonging to the authenticated participant.
        [HttpGet("mine")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int participantId) ||
                participantId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Unable to identify the authenticated participant."
                });
            }

            var enrolments = new List<EnrolmentDto>();

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
            SELECT
                EnrolmentID,
                ParticipantID,
                EventID,
                CategoryID,
                EnrolmentDate,
                EnrolmentStatus
            FROM dbo.ENROLMENT
            WHERE ParticipantID = @ParticipantID
            ORDER BY EnrolmentDate DESC;";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters.Add("@ParticipantID", SqlDbType.Int)
                    .Value = participantId;

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    enrolments.Add(new EnrolmentDto
                    {
                        EnrolmentId = reader.GetInt32(0),
                        ParticipantId = reader.GetInt32(1),
                        EventId = reader.GetInt32(2),
                        CategoryId = reader.GetInt32(3),
                        EnrolmentDate = reader.GetDateTime(4),
                        EnrolmentStatus = reader.GetString(5)
                    });
                }

                return Ok(enrolments);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while retrieving your enrolments."
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

        // GET: api/Enrolments/{id}
        // Retrieves an enrolment only when it belongs to the authenticated participant.
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetEnrolmentById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Enrolment ID must be greater than zero."
                });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")
                ?? User.FindFirst("userId");

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int participantId) ||
                participantId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Unable to identify the authenticated participant."
                });
            }

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    SELECT
                        EnrolmentID,
                        ParticipantID,
                        EventID,
                        CategoryID,
                        EnrolmentDate,
                        EnrolmentStatus
                    FROM dbo.ENROLMENT
                    WHERE EnrolmentID = @EnrolmentID
                      AND ParticipantID = @ParticipantID;";

                using var command = new SqlCommand(sql, _connection);
                command.Parameters.Add("@EnrolmentID", SqlDbType.Int).Value = id;
                command.Parameters.Add("@ParticipantID", SqlDbType.Int).Value = participantId;

                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        message = $"Enrolment with ID {id} was not found."
                    });
                }

                var enrolment = new EnrolmentDto
                {
                    EnrolmentId = reader.GetInt32(0),
                    ParticipantId = reader.GetInt32(1),
                    EventId = reader.GetInt32(2),
                    CategoryId = reader.GetInt32(3),
                    EnrolmentDate = reader.GetDateTime(4),
                    EnrolmentStatus = reader.GetString(5)
                };

                return Ok(enrolment);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while retrieving the enrolment."
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

        // POST: api/Enrolments
        // Registers the authenticated participant for an event category.
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEnrolment(
            [FromBody] CreateEnrolmentDto request)
        {
            // Get the participant ID from the authenticated JWT.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")
                ?? User.FindFirst("userId");

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int participantId) ||
                participantId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Unable to identify the authenticated participant."
                });
            }

            try
            {
                await _connection.OpenAsync();

                // Check that the event exists and that the category belongs to it.
                const string validationSql = @"
            SELECT COUNT(1)
            FROM dbo.CATEGORY
            WHERE CategoryID = @CategoryID
              AND EventID = @EventID;";

                using var validationCommand =
                    new SqlCommand(validationSql, _connection);

                validationCommand.Parameters
                    .Add("@CategoryID", SqlDbType.Int).Value = request.CategoryId;

                validationCommand.Parameters
                    .Add("@EventID", SqlDbType.Int).Value = request.EventId;

                var validCategory = Convert.ToInt32(
                    await validationCommand.ExecuteScalarAsync()) > 0;

                if (!validCategory)
                {
                    return BadRequest(new
                    {
                        message = "The specified category does not belong to the selected event or does not exist."
                    });
                }

                // Prevent duplicate enrolments for the same participant,
                // event and category.
                const string duplicateSql = @"
            SELECT COUNT(1)
            FROM dbo.ENROLMENT
            WHERE ParticipantID = @ParticipantID
              AND EventID = @EventID
              AND CategoryID = @CategoryID;";

                using var duplicateCommand =
                    new SqlCommand(duplicateSql, _connection);

                duplicateCommand.Parameters
                    .Add("@ParticipantID", SqlDbType.Int).Value = participantId;

                duplicateCommand.Parameters
                    .Add("@EventID", SqlDbType.Int).Value = request.EventId;

                duplicateCommand.Parameters
                    .Add("@CategoryID", SqlDbType.Int).Value = request.CategoryId;

                var duplicateExists = Convert.ToInt32(
                    await duplicateCommand.ExecuteScalarAsync()) > 0;

                if (duplicateExists)
                {
                    return Conflict(new
                    {
                        message = "You are already enrolled in this category for this event."
                    });
                }

                // Let SQL Server supply EnrolmentDate and EnrolmentStatus
                // using the defaults defined in the database.
                const string insertSql = @"
            INSERT INTO dbo.ENROLMENT
            (
                ParticipantID,
                EventID,
                CategoryID
            )
            OUTPUT
                INSERTED.EnrolmentID,
                INSERTED.ParticipantID,
                INSERTED.EventID,
                INSERTED.CategoryID,
                INSERTED.EnrolmentDate,
                INSERTED.EnrolmentStatus
            VALUES
            (
                @ParticipantID,
                @EventID,
                @CategoryID
            );";

                using var insertCommand =
                    new SqlCommand(insertSql, _connection);

                insertCommand.Parameters
                    .Add("@ParticipantID", SqlDbType.Int).Value = participantId;

                insertCommand.Parameters
                    .Add("@EventID", SqlDbType.Int).Value = request.EventId;

                insertCommand.Parameters
                    .Add("@CategoryID", SqlDbType.Int).Value = request.CategoryId;

                using var reader = await insertCommand.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new
                        {
                            message = "The enrolment could not be created."
                        });
                }

                var createdEnrolment = new EnrolmentDto
                {
                    EnrolmentId = reader.GetInt32(0),
                    ParticipantId = reader.GetInt32(1),
                    EventId = reader.GetInt32(2),
                    CategoryId = reader.GetInt32(3),
                    EnrolmentDate = reader.GetDateTime(4),
                    EnrolmentStatus = reader.GetString(5)
                };

                return StatusCode(
                    StatusCodes.Status201Created,
                    createdEnrolment);
            }
            catch (SqlException ex) when (
                ex.Number == 2601 || ex.Number == 2627)
            {
                return Conflict(new
                {
                    message = "An enrolment for this participant, event and category already exists."
                });
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return BadRequest(new
                {
                    message = "The participant, event or category does not exist or violates a database relationship."
                });
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while creating the enrolment."
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

        // GET: api/Enrolments/event/{eventId}
        // Retrieves enrolments for an event managed by the authenticated organiser.
        [HttpGet("event/{eventId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEnrolmentsByEvent(int eventId)
        {
            if (eventId <= 0)
            {
                return BadRequest(new
                {
                    message = "Event ID must be greater than zero."
                });
            }

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

            try
            {
                await _connection.OpenAsync();

                // Confirm that this event belongs to the authenticated organiser.
                const string checkEventSql = @"
            SELECT COUNT(1)
            FROM dbo.EVENT
            WHERE EventID = @EventID
              AND OrganiserID = @OrganiserID;";

                using var checkCommand =
                    new SqlCommand(checkEventSql, _connection);

                checkCommand.Parameters.Add("@EventID", SqlDbType.Int)
                    .Value = eventId;

                checkCommand.Parameters.Add("@OrganiserID", SqlDbType.Int)
                    .Value = organiserId;

                var eventExists = Convert.ToInt32(
                    await checkCommand.ExecuteScalarAsync()) > 0;

                if (!eventExists)
                {
                    return NotFound(new
                    {
                        message = "The event was not found or is not managed by you."
                    });
                }

                // Retrieve enrolments for the authorised event.
                const string sql = @"
            SELECT
                EnrolmentID,
                ParticipantID,
                EventID,
                CategoryID,
                EnrolmentDate,
                EnrolmentStatus
            FROM dbo.ENROLMENT
            WHERE EventID = @EventID
            ORDER BY EnrolmentDate DESC;";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters.Add("@EventID", SqlDbType.Int)
                    .Value = eventId;

                var enrolments = new List<EnrolmentDto>();

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    enrolments.Add(new EnrolmentDto
                    {
                        EnrolmentId = reader.GetInt32(0),
                        ParticipantId = reader.GetInt32(1),
                        EventId = reader.GetInt32(2),
                        CategoryId = reader.GetInt32(3),
                        EnrolmentDate = reader.GetDateTime(4),
                        EnrolmentStatus = reader.GetString(5)
                    });
                }

                return Ok(enrolments);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while retrieving event enrolments."
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

        // PUT: api/Enrolments/{id}
        // Updates the status of an enrolment.
        [HttpPut("{id:int}")]
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEnrolment(
            int id,
            [FromBody] UpdateEnrolmentDto request)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Enrolment ID must be greater than zero."
                });
            }

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

                // Confirm that the enrolment belongs to an event
                // managed by this organiser.
                const string checkSql = @"
            SELECT COUNT(1)
            FROM dbo.ENROLMENT EN
            INNER JOIN dbo.EVENT E
                ON E.EventID = EN.EventID
            WHERE EN.EnrolmentID = @EnrolmentID
              AND E.OrganiserID = @OrganiserID;";

                using var checkCommand =
                    new SqlCommand(checkSql, _connection);

                checkCommand.Parameters
                    .Add("@EnrolmentID", SqlDbType.Int).Value = id;

                checkCommand.Parameters
                    .Add("@OrganiserID", SqlDbType.Int).Value = organiserId;

                var enrolmentExists = Convert.ToInt32(
                    await checkCommand.ExecuteScalarAsync()) > 0;

                if (!enrolmentExists)
                {
                    return NotFound(new
                    {
                        message = "The enrolment was not found or does not belong to an event managed by you."
                    });
                }

                const string updateSql = @"
            UPDATE dbo.ENROLMENT
            SET EnrolmentStatus = @EnrolmentStatus
            OUTPUT
                INSERTED.EnrolmentID,
                INSERTED.ParticipantID,
                INSERTED.EventID,
                INSERTED.CategoryID,
                INSERTED.EnrolmentDate,
                INSERTED.EnrolmentStatus
            WHERE EnrolmentID = @EnrolmentID
              AND EventID IN
              (
                  SELECT EventID
                  FROM dbo.EVENT
                  WHERE OrganiserID = @OrganiserID
              );";

                using var updateCommand =
                    new SqlCommand(updateSql, _connection);

                updateCommand.Parameters
                    .Add("@EnrolmentID", SqlDbType.Int).Value = id;

                updateCommand.Parameters
                    .Add("@OrganiserID", SqlDbType.Int).Value = organiserId;

                updateCommand.Parameters
                    .Add("@EnrolmentStatus", SqlDbType.VarChar, 30)
                    .Value = request.EnrolmentStatus;

                using var reader = await updateCommand.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        message = "The enrolment could not be found or updated."
                    });
                }

                var updatedEnrolment = new EnrolmentDto
                {
                    EnrolmentId = reader.GetInt32(0),
                    ParticipantId = reader.GetInt32(1),
                    EventId = reader.GetInt32(2),
                    CategoryId = reader.GetInt32(3),
                    EnrolmentDate = reader.GetDateTime(4),
                    EnrolmentStatus = reader.GetString(5)
                };

                return Ok(updatedEnrolment);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while updating the enrolment."
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
        // DELETE: api/Enrolments/{id}
        // Cancels an enrolment belonging to the authenticated participant.
        // The record is retained so any related result remains linked to it.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Enrolment ID must be greater than zero."
                });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")
                ?? User.FindFirst("userId");

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int participantId) ||
                participantId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Unable to identify the authenticated participant."
                });
            }

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    UPDATE dbo.ENROLMENT
                    SET EnrolmentStatus = 'Cancelled'
                    WHERE EnrolmentID = @EnrolmentID
                      AND ParticipantID = @ParticipantID
                      AND EnrolmentStatus <> 'Cancelled';";

                using var command = new SqlCommand(sql, _connection);
                command.Parameters.Add("@EnrolmentID", SqlDbType.Int).Value = id;
                command.Parameters.Add("@ParticipantID", SqlDbType.Int).Value = participantId;

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    // Do not reveal whether an enrolment belonging to another
                    // participant exists. This also reports an already-cancelled
                    // enrolment as not found for this operation.
                    return NotFound(new
                    {
                        message = "The enrolment was not found, does not belong to you, or is already cancelled."
                    });
                }

                return Ok(new
                {
                    message = $"Enrolment with ID {id} was cancelled successfully."
                });
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while cancelling the enrolment."
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
