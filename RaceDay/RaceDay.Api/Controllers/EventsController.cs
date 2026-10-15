using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using RaceDay.contracts.DTOs;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly SqlConnection _connection;

        public EventsController(SqlConnection connection)
        {
            _connection = connection;
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
                    SELECT EventID, EventName, EventDate, Location
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
                        Location = reader.GetString(3)
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
        // Retrieves one event using its ID.
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
                    SELECT EventID, EventName, EventDate, Location
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

                var eventDto = new EventDto
                {
                    EventId = reader.GetInt32(0),
                    EventName = reader.GetString(1),
                    EventDate = reader.GetDateTime(2),
                    Location = reader.GetString(3)
                };

                return Ok(eventDto);
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
    }
}