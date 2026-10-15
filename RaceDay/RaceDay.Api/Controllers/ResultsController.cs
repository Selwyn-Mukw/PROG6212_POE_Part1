using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay.contracts.DTOs;
using System.Data;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly SqlConnection _connection;

        public ResultsController(SqlConnection connection)
        {
            _connection = connection;
        }

        // GET: api/Results
        // Retrieves all results.
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            var results = new List<ResultDto>();

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    SELECT
                        ResultID,
                        EnrolmentID,
                        FinishTime,
                        FinishPosition,
                        ResultStatus
                    FROM dbo.RESULT
                    ORDER BY FinishPosition;";

                using var command = new SqlCommand(sql, _connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(new ResultDto
                    {
                        ResultId = reader.GetInt32(0),
                        EnrolmentId = reader.GetInt32(1),
                        FinishTime = reader.GetTimeSpan(2),
                        FinishPosition = reader.GetInt32(3),
                        ResultStatus = reader.GetString(4)
                    });
                }

                return Ok(results);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while retrieving results."
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

        // GET: api/Results/{id}
        // Retrieves one result by its ID.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetResultById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Result ID must be greater than zero."
                });
            }

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    SELECT
                        ResultID,
                        EnrolmentID,
                        FinishTime,
                        FinishPosition,
                        ResultStatus
                    FROM dbo.RESULT
                    WHERE ResultID = @ResultID;";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters
                    .Add("@ResultID", SqlDbType.Int).Value = id;

                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        message = $"Result with ID {id} was not found."
                    });
                }

                var result = new ResultDto
                {
                    ResultId = reader.GetInt32(0),
                    EnrolmentId = reader.GetInt32(1),
                    FinishTime = reader.GetTimeSpan(2),
                    FinishPosition = reader.GetInt32(3),
                    ResultStatus = reader.GetString(4)
                };

                return Ok(result);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while retrieving the result."
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

        // POST: api/Results
        // Creates a result for an enrolment managed by the authenticated organiser.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(
            [FromBody] CreateResultDto request)
        {
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

                // Verify that the enrolment belongs to an event
                // managed by this organiser.
                const string enrolmentSql = @"
                    SELECT COUNT(1)
                    FROM dbo.ENROLMENT EN
                    INNER JOIN dbo.EVENT E
                        ON E.EventID = EN.EventID
                    WHERE EN.EnrolmentID = @EnrolmentID
                      AND E.OrganiserID = @OrganiserID;";

                using var enrolmentCommand =
                    new SqlCommand(enrolmentSql, _connection);

                enrolmentCommand.Parameters
                    .Add("@EnrolmentID", SqlDbType.Int)
                    .Value = request.EnrolmentId;

                enrolmentCommand.Parameters
                    .Add("@OrganiserID", SqlDbType.Int)
                    .Value = organiserId;

                var enrolmentExists = Convert.ToInt32(
                    await enrolmentCommand.ExecuteScalarAsync()) > 0;

                if (!enrolmentExists)
                {
                    return NotFound(new
                    {
                        message = "The enrolment was not found or does not belong to an event managed by you."
                    });
                }

                // Prevent multiple results for the same enrolment.
                const string duplicateSql = @"
                    SELECT COUNT(1)
                    FROM dbo.RESULT
                    WHERE EnrolmentID = @EnrolmentID;";

                using var duplicateCommand =
                    new SqlCommand(duplicateSql, _connection);

                duplicateCommand.Parameters
                    .Add("@EnrolmentID", SqlDbType.Int)
                    .Value = request.EnrolmentId;

                var resultExists = Convert.ToInt32(
                    await duplicateCommand.ExecuteScalarAsync()) > 0;

                if (resultExists)
                {
                    return Conflict(new
                    {
                        message = "A result already exists for this enrolment."
                    });
                }

                const string insertSql = @"
                    INSERT INTO dbo.RESULT
                    (
                        EnrolmentID,
                        FinishTime,
                        FinishPosition
                    )
                    OUTPUT
                        INSERTED.ResultID,
                        INSERTED.EnrolmentID,
                        INSERTED.FinishTime,
                        INSERTED.FinishPosition,
                        INSERTED.ResultStatus
                    VALUES
                    (
                        @EnrolmentID,
                        @FinishTime,
                        @FinishPosition
                    );";

                using var insertCommand =
                    new SqlCommand(insertSql, _connection);

                insertCommand.Parameters
                    .Add("@EnrolmentID", SqlDbType.Int)
                    .Value = request.EnrolmentId;

                insertCommand.Parameters
                    .Add("@FinishTime", SqlDbType.Time)
                    .Value = request.FinishTime;

                insertCommand.Parameters
                    .Add("@FinishPosition", SqlDbType.Int)
                    .Value = request.FinishPosition;

                using var reader =
                    await insertCommand.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new
                        {
                            message = "The result could not be created."
                        });
                }

                var createdResult = new ResultDto
                {
                    ResultId = reader.GetInt32(0),
                    EnrolmentId = reader.GetInt32(1),
                    FinishTime = reader.GetTimeSpan(2),
                    FinishPosition = reader.GetInt32(3),
                    ResultStatus = reader.GetString(4)
                };

                return StatusCode(
                    StatusCodes.Status201Created,
                    createdResult);
            }
            catch (SqlException ex) when (
                ex.Number == 2601 || ex.Number == 2627)
            {
                return Conflict(new
                {
                    message = "A result already exists for this enrolment."
                });
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return BadRequest(new
                {
                    message = "The enrolment does not exist or violates a database relationship."
                });
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while creating the result."
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

        // PUT: api/Results/{id}
        // Updates a result belonging to an event managed by the authenticated organiser.
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(
            int id,
            [FromBody] CreateResultDto request)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Result ID must be greater than zero."
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

                // Verify that the result belongs to an event
                // managed by the authenticated organiser.
                const string ownershipSql = @"
                    SELECT COUNT(1)
                    FROM dbo.RESULT R
                    INNER JOIN dbo.ENROLMENT EN
                        ON EN.EnrolmentID = R.EnrolmentID
                    INNER JOIN dbo.EVENT E
                        ON E.EventID = EN.EventID
                    WHERE R.ResultID = @ResultID
                      AND E.OrganiserID = @OrganiserID;";

                using var ownershipCommand =
                    new SqlCommand(ownershipSql, _connection);

                ownershipCommand.Parameters
                    .Add("@ResultID", SqlDbType.Int).Value = id;

                ownershipCommand.Parameters
                    .Add("@OrganiserID", SqlDbType.Int).Value = organiserId;

                var resultBelongsToOrganiser = Convert.ToInt32(
                    await ownershipCommand.ExecuteScalarAsync()) > 0;

                if (!resultBelongsToOrganiser)
                {
                    return NotFound(new
                    {
                        message = "The result was not found or does not belong to an event managed by you."
                    });
                }

                const string updateSql = @"
                    UPDATE dbo.RESULT
                    SET
                        FinishTime = @FinishTime,
                        FinishPosition = @FinishPosition
                    OUTPUT
                        INSERTED.ResultID,
                        INSERTED.EnrolmentID,
                        INSERTED.FinishTime,
                        INSERTED.FinishPosition,
                        INSERTED.ResultStatus
                    WHERE ResultID = @ResultID;";

                using var updateCommand =
                    new SqlCommand(updateSql, _connection);

                updateCommand.Parameters
                    .Add("@ResultID", SqlDbType.Int).Value = id;

                updateCommand.Parameters
                    .Add("@FinishTime", SqlDbType.Time).Value =
                        request.FinishTime;

                updateCommand.Parameters
                    .Add("@FinishPosition", SqlDbType.Int).Value =
                        request.FinishPosition;

                using var reader =
                    await updateCommand.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        message = "The result could not be found."
                    });
                }

                var updatedResult = new ResultDto
                {
                    ResultId = reader.GetInt32(0),
                    EnrolmentId = reader.GetInt32(1),
                    FinishTime = reader.GetTimeSpan(2),
                    FinishPosition = reader.GetInt32(3),
                    ResultStatus = reader.GetString(4)
                };

                return Ok(updatedResult);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while updating the result."
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
        // DELETE: api/Results/{id}
        // Deletes a result belonging to an event managed by the authenticated organiser.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Result ID must be greater than zero."
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

                const string sql = @"
                    DELETE R
                    OUTPUT DELETED.ResultID
                    FROM dbo.RESULT R
                    INNER JOIN dbo.ENROLMENT EN
                        ON EN.EnrolmentID = R.EnrolmentID
                    INNER JOIN dbo.EVENT E
                        ON E.EventID = EN.EventID
                    WHERE R.ResultID = @ResultID
                      AND E.OrganiserID = @OrganiserID;";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters
                    .Add("@ResultID", SqlDbType.Int).Value = id;

                command.Parameters
                    .Add("@OrganiserID", SqlDbType.Int).Value = organiserId;

                var deletedResultId = await command.ExecuteScalarAsync();

                if (deletedResultId == null || deletedResultId == DBNull.Value)
                {
                    return NotFound(new
                    {
                        message = "The result was not found or does not belong to an event managed by you."
                    });
                }

                return Ok(new
                {
                    message = $"Result with ID {id} was deleted successfully."
                });
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while deleting the result."
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