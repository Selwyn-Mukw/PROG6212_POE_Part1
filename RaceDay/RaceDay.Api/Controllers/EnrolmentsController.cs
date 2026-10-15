using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using RaceDay.contracts.DTOs;

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
        // Retrieves all enrolments.
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
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
                    ORDER BY EnrolmentDate DESC;";

                using var command = new SqlCommand(sql, _connection);

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
                        message = "A database error occurred while retrieving enrolments."
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
        // Retrieves one enrolment by its ID.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEnrolmentById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Enrolment ID must be greater than zero."
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
            WHERE EnrolmentID = @EnrolmentID;";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters.Add("@EnrolmentID", SqlDbType.Int).Value = id;

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
    }
}