using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;
using RaceDay.contracts.DTOs;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiClient _api;

        public OrganiserController(RaceDayApiClient api)
        {
            _api = api;
        }

        // ==========================================
        // EVENT TYPES
        // ==========================================

        private async Task<List<SelectListItem>> GetEventTypesAsync()
        {
            using var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/Events/types");

            if (!response.IsSuccessStatusCode)
            {
                return new List<SelectListItem>();
            }

            var eventTypes =
                await _api.ReadAsync<List<EventTypeVm>>(response);

            return eventTypes?.Select(type => new SelectListItem
            {
                Value = type.EventTypeId.ToString(),
                Text = type.EventTypeName
            }).ToList() ?? new List<SelectListItem>();
        }

        // ==========================================
        // AUTHENTICATION CHECK
        // ==========================================

        private bool IsOrganiser()
        {
            string? token = HttpContext.Session.GetString("JwtToken");
            string? role = HttpContext.Session.GetString("Role");

            return !string.IsNullOrEmpty(token) && role == "Organiser";
        }

        // ==========================================
        // ORGANISER DASHBOARD
        // ==========================================

        public IActionResult Index()
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.DisplayName =
                HttpContext.Session.GetString("DisplayName");

            return View();
        }

        // ==========================================
        // VIEW EVENTS
        // ==========================================

        public async Task<IActionResult> Events()
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            using var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/Events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to retrieve events.";
                return View(new List<EventVm>());
            }

            var events =
                await _api.ReadAsync<List<EventVm>>(response);

            return View(events ?? new List<EventVm>());
        }

        // ==========================================
        // CREATE EVENT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new CreateEventVm
            {
                EventTypes = await GetEventTypesAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventVm model)
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                model.EventTypes = await GetEventTypesAsync();
                return View(model);
            }

            var eventDto = new CreateEventDto
            {
                EventName = model.EventName,
                EventDate = model.EventDate,
                Location = model.Location,
                Description = model.Description,
                Distance = model.Distance,
                EventTypeId = model.EventTypeId
            };

            using var response = await _api.SendAsync(
                HttpMethod.Post,
                "api/Events",
                eventDto);

            if (!response.IsSuccessStatusCode)
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                ViewBag.Error = string.IsNullOrWhiteSpace(error)
                    ? "Unable to create the event. Please try again."
                    : error;

                model.EventTypes = await GetEventTypesAsync();
                return View(model);
            }

            TempData["Success"] = "Event created successfully!";

            return RedirectToAction("Events");
        }

        // ==========================================
        // EDIT EVENT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            using var response = await _api.SendAsync(
                HttpMethod.Get,
                $"api/Events/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to retrieve the selected event.";

                return RedirectToAction("Events");
            }

            var model =
                await _api.ReadAsync<EditEventVm>(response);

            if (model == null)
            {
                TempData["Error"] =
                    "Event details could not be loaded.";

                return RedirectToAction("Events");
            }

            model.EventTypes = await GetEventTypesAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(EditEventVm model)
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                model.EventTypes = await GetEventTypesAsync();
                return View(model);
            }

            var eventDto = new UpdateEventDto
            {
                EventName = model.EventName,
                EventDate = model.EventDate,
                Location = model.Location,
                Description = model.Description,
                Distance = model.Distance,
                EventTypeId = model.EventTypeId
            };

            using var response = await _api.SendAsync(
                HttpMethod.Put,
                $"api/Events/{model.EventId}",
                eventDto);

            if (!response.IsSuccessStatusCode)
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                ViewBag.Error = string.IsNullOrWhiteSpace(error)
                    ? "Unable to update the event. Please try again."
                    : error;

                model.EventTypes = await GetEventTypesAsync();

                return View(model);
            }

            TempData["Success"] = "Event updated successfully!";

            return RedirectToAction("Events");
        }

        // ==========================================
        // DELETE EVENT
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            using var response = await _api.SendAsync(
                HttpMethod.Delete,
                $"api/Events/{id}");

            if (!response.IsSuccessStatusCode)
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                TempData["Error"] =
                    "Unable to delete event. " + error;

                return RedirectToAction("Events");
            }

            TempData["Success"] = "Event deleted successfully!";

            return RedirectToAction("Events");
        }

        // ==========================================
        // CATEGORY MANAGEMENT
        // ==========================================

        // Retrieve events belonging to the logged-in organiser.
        private async Task<List<SelectListItem>> GetOrganiserEventsAsync()
        {
            using var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/Events/mine");

            if (!response.IsSuccessStatusCode)
            {
                return new List<SelectListItem>();
            }

            var events =
                await _api.ReadAsync<List<EventVm>>(response);

            return events?.Select(e => new SelectListItem
            {
                Value = e.EventId.ToString(),
                Text = e.EventName
            }).ToList() ?? new List<SelectListItem>();
        }

        // Retrieve categories belonging to a selected event.
        private async Task<List<CategoryDto>> GetCategoriesAsync(int eventId)
        {
            using var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/Categories");

            if (!response.IsSuccessStatusCode)
            {
                return new List<CategoryDto>();
            }

            var categories =
                await _api.ReadAsync<List<CategoryDto>>(response);

            return categories?
                .Where(c => c.EventId == eventId)
                .ToList() ?? new List<CategoryDto>();
        }

        // Display Category Management page.
        [HttpGet]
        public async Task<IActionResult> Categories(int? eventId)
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new CategoryVm
            {
                Events = await GetOrganiserEventsAsync()
            };

            if (eventId.HasValue)
            {
                model.EventId = eventId.Value;
            }
            else if (model.Events.Any())
            {
                model.EventId = int.Parse(model.Events.First().Value);
            }

            if (model.EventId > 0)
            {
                model.Categories =
                    await GetCategoriesAsync(model.EventId);
            }

            return View(model);
        }

        // Create a new category.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Categories(CategoryVm model)
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                model.Events = await GetOrganiserEventsAsync();

                if (model.EventId > 0)
                {
                    model.Categories =
                        await GetCategoriesAsync(model.EventId);
                }

                return View(model);
            }

            if (model.MinimumAge > model.MaximumAge)
            {
                ModelState.AddModelError(
                    "MaximumAge",
                    "Maximum age must be greater than or equal to minimum age.");

                model.Events = await GetOrganiserEventsAsync();

                model.Categories =
                    await GetCategoriesAsync(model.EventId);

                return View(model);
            }

            var categoryDto = new CreateCategoryDto
            {
                EventId = model.EventId,
                CategoryName = model.CategoryName,
                CategoryType = model.CategoryType,
                MinimumAge = model.MinimumAge,
                MaximumAge = model.MaximumAge,
                CategoryDistance = model.CategoryDistance
            };

            using var response = await _api.SendAsync(
                HttpMethod.Post,
                "api/Categories",
                categoryDto);

            if (!response.IsSuccessStatusCode)
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                ViewBag.Error = string.IsNullOrWhiteSpace(error)
                    ? "Unable to create the category."
                    : error;

                model.Events = await GetOrganiserEventsAsync();

                model.Categories =
                    await GetCategoriesAsync(model.EventId);

                return View(model);
            }

            TempData["Success"] = "Category created successfully!";

            return RedirectToAction(
                "Categories",
                new { eventId = model.EventId });
        }

        // ==========================================
        // EDIT CATEGORY
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            using var response = await _api.SendAsync(
                HttpMethod.Get,
                $"api/Categories/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to retrieve the selected category.";

                return RedirectToAction("Categories");
            }

            var category =
                await _api.ReadAsync<CategoryDto>(response);

            if (category == null)
            {
                TempData["Error"] =
                    "Category details could not be loaded.";

                return RedirectToAction("Categories");
            }

            // Confirm the category belongs to one of this organiser's events.
            var organiserEvents = await GetOrganiserEventsAsync();

            bool ownsEvent = organiserEvents.Any(e =>
                e.Value == category.EventId.ToString());

            if (!ownsEvent)
            {
                TempData["Error"] =
                    "You are not authorised to edit this category.";

                return RedirectToAction("Categories");
            }

            var model = new EditCategoryVm
            {
                CategoryId = category.CategoryId,
                EventId = category.EventId,
                CategoryName = category.CategoryName,
                CategoryType = category.CategoryType,
                MinimumAge = category.MinimumAge,
                MaximumAge = category.MaximumAge,
                CategoryDistance = category.CategoryDistance
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(EditCategoryVm model)
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (model.MinimumAge > model.MaximumAge)
            {
                ModelState.AddModelError(
                    "MaximumAge",
                    "Maximum age must be greater than or equal to minimum age.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var categoryDto = new UpdateCategoryDto
            {
                CategoryName = model.CategoryName,
                CategoryType = model.CategoryType,
                MinimumAge = model.MinimumAge,
                MaximumAge = model.MaximumAge,
                CategoryDistance = model.CategoryDistance
            };

            using var response = await _api.SendAsync(
                HttpMethod.Put,
                $"api/Categories/{model.CategoryId}",
                categoryDto);

            if (!response.IsSuccessStatusCode)
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                ViewBag.Error = string.IsNullOrWhiteSpace(error)
                    ? "Unable to update the category."
                    : error;

                return View(model);
            }

            TempData["Success"] =
                "Category updated successfully!";

            return RedirectToAction(
                "Categories",
                new { eventId = model.EventId });
        }

        // ==========================================
        // DELETE CATEGORY
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id, int eventId)
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            using var response = await _api.SendAsync(
                HttpMethod.Delete,
                $"api/Categories/{id}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    TempData["Error"] =
                        "This category cannot be deleted because participants are already enrolled in it.";
                }
                else
                {
                    TempData["Error"] =
                        "Unable to delete this category. Please try again.";
                }

                return RedirectToAction(
                    "Categories",
                    new { eventId });
            }

            TempData["Success"] =
                "Category deleted successfully!";

            return RedirectToAction(
                "Categories",
                new { eventId });
        }
    }
}