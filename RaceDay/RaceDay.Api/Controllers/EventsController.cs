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
    public class EventsController : ControllerBase
    {
        private readonly SqlConnection _connection;

        public EventsController(SqlConnection connection)
        {
            _connection = connection;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = new List<EventDto>();

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    SELECT
                        EventID,
                        EventName,
                        EventDate,
                        Location
                    FROM dbo.EVENT
                    ORDER BY EventDate;";

                using var command = new SqlCommand(
                    sql, _connection);

                using var reader =
                    await command.ExecuteReaderAsync();

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
                return StatusCode(500, new
                {
                    message =
                        "A database error occurred while retrieving events."
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
                    SELECT
                        EventID,
                        EventName,
                        EventDate,
                        Location
                    FROM dbo.EVENT
                    WHERE EventID = @EventID;";

                using var command = new SqlCommand(
                    sql, _connection);

                command.Parameters.Add(
                    "@EventID", SqlDbType.Int).Value = id;

                using var reader =
                    await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        message =
                            $"Event with ID {id} was not found."
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
                return StatusCode(500, new
                {
                    message =
                        "A database error occurred while retrieving the event."
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