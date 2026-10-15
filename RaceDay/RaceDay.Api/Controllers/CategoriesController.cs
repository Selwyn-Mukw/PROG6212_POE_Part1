using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay.contracts.DTOs;
using System.Data;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly SqlConnection _connection;

        public CategoriesController(SqlConnection connection)
        {
            _connection = connection;
        }

        // GET: api/Categories
        // Retrieves all categories.
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = new List<CategoryDto>();

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
                    SELECT
                        CategoryID,
                        EventID,
                        CategoryName,
                        CategoryType,
                        MinimumAge,
                        MaximumAge,
                        CategoryDistance
                    FROM dbo.CATEGORY
                    ORDER BY CategoryID;";

                using var command = new SqlCommand(sql, _connection);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    categories.Add(new CategoryDto
                    {
                        CategoryId = reader.GetInt32(0),
                        EventId = reader.GetInt32(1),
                        CategoryName = reader.GetString(2),
                        CategoryType = reader.GetString(3),
                        MinimumAge = reader.GetInt32(4),
                        MaximumAge = reader.GetInt32(5),
                        CategoryDistance = reader.GetDecimal(6)
                    });
                }

                return Ok(categories);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while retrieving categories."
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
        // GET: api/Categories/{id}
        // Retrieves one category using its ID.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Category ID must be greater than zero."
                });
            }

            try
            {
                await _connection.OpenAsync();

                const string sql = @"
            SELECT
                CategoryID,
                EventID,
                CategoryName,
                CategoryType,
                MinimumAge,
                MaximumAge,
                CategoryDistance
            FROM dbo.CATEGORY
            WHERE CategoryID = @CategoryID;";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = id;

                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        message = $"Category with ID {id} was not found."
                    });
                }

                var category = new CategoryDto
                {
                    CategoryId = reader.GetInt32(0),
                    EventId = reader.GetInt32(1),
                    CategoryName = reader.GetString(2),
                    CategoryType = reader.GetString(3),
                    MinimumAge = reader.GetInt32(4),
                    MaximumAge = reader.GetInt32(5),
                    CategoryDistance = reader.GetDecimal(6)
                };

                return Ok(category);
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while retrieving the category."
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
        // POST: api/Categories
        // Creates a category for an event.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(
            [FromBody] CreateCategoryDto request)
        {
            if (request.MinimumAge > request.MaximumAge)
            {
                return BadRequest(new
                {
                    message = "Minimum age cannot exceed maximum age."
                });
            }

            var userIdClaim = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)
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

                // Confirm that the event exists and belongs to this organiser.
                const string eventSql = @"
            SELECT COUNT(1)
            FROM dbo.EVENT
            WHERE EventID = @EventID
              AND OrganiserID = @OrganiserID;";

                using var eventCommand = new SqlCommand(eventSql, _connection);

                eventCommand.Parameters.Add("@EventID", SqlDbType.Int)
                    .Value = request.EventId;

                eventCommand.Parameters.Add("@OrganiserID", SqlDbType.Int)
                    .Value = organiserId;

                var eventExists = Convert.ToInt32(
                    await eventCommand.ExecuteScalarAsync()) > 0;

                if (!eventExists)
                {
                    return BadRequest(new
                    {
                        message = "The event does not exist or does not belong to this organiser."
                    });
                }

                const string sql = @"
            INSERT INTO dbo.CATEGORY
            (
                EventID,
                CategoryName,
                CategoryType,
                MinimumAge,
                MaximumAge,
                CategoryDistance
            )
            OUTPUT
                INSERTED.CategoryID,
                INSERTED.EventID,
                INSERTED.CategoryName,
                INSERTED.CategoryType,
                INSERTED.MinimumAge,
                INSERTED.MaximumAge,
                INSERTED.CategoryDistance
            VALUES
            (
                @EventID,
                @CategoryName,
                @CategoryType,
                @MinimumAge,
                @MaximumAge,
                @CategoryDistance
            );";

                using var command = new SqlCommand(sql, _connection);

                command.Parameters.Add("@EventID", SqlDbType.Int)
                    .Value = request.EventId;

                command.Parameters.Add("@CategoryName", SqlDbType.VarChar, 100)
                    .Value = request.CategoryName;

                command.Parameters.Add("@CategoryType", SqlDbType.VarChar, 50)
                    .Value = request.CategoryType;

                command.Parameters.Add("@MinimumAge", SqlDbType.Int)
                    .Value = request.MinimumAge;

                command.Parameters.Add("@MaximumAge", SqlDbType.Int)
                    .Value = request.MaximumAge;

                var distanceParameter = command.Parameters.Add(
                    "@CategoryDistance", SqlDbType.Decimal);

                distanceParameter.Precision = 6;
                distanceParameter.Scale = 2;
                distanceParameter.Value = request.CategoryDistance;

                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new
                        {
                            message = "The category could not be created."
                        });
                }

                var createdCategory = new CategoryDto
                {
                    CategoryId = reader.GetInt32(0),
                    EventId = reader.GetInt32(1),
                    CategoryName = reader.GetString(2),
                    CategoryType = reader.GetString(3),
                    MinimumAge = reader.GetInt32(4),
                    MaximumAge = reader.GetInt32(5),
                    CategoryDistance = reader.GetDecimal(6)
                };

                return StatusCode(
                    StatusCodes.Status201Created,
                    createdCategory);
            }
            catch (SqlException ex) when (
                ex.Number == 2601 || ex.Number == 2627)
            {
                return Conflict(new
                {
                    message = "A category with this name already exists for the selected event."
                });
            }
            catch (SqlException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "A database error occurred while creating the category."
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