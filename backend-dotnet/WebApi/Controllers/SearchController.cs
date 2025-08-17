using Application.Events;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Diagnostics;
using System.Security.Claims;
using WebApi.Extensions;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly IEventService _eventService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            ISearchService searchService,
            IEventService eventService,
            ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _eventService = eventService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { error = "Query is required." });

            var stopwatch = Stopwatch.StartNew();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var result = await _searchService.SearchAsync(query, userId);
                stopwatch.Stop();

                var searchEvent = new SearchEvent
                {
                    SearchTerm = query,
                    SearchType = query.StartsWith('#') ? "hashtag" : "general",
                    SearchDurationMs = stopwatch.ElapsedMilliseconds,
                    ResultCount = GetResultCount(result),
                    Metadata = new Dictionary<string, object>
                    {
                        { "hasResults", result != null },
                        { "queryLength", query.Length }
                    }
                };

                HttpContext.EnrichEvent(searchEvent);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _eventService.PublishSearchEventAsync(searchEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to publish search event for term: {SearchTerm}", query);
                    }
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error performing search for term: {SearchTerm}", query);
                return StatusCode(500, "An error occurred while performing the search");
            }
        }

        [HttpGet("autocomplete")]
        [AllowAnonymous]
        public async Task<IActionResult> Autocomplete([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query is required.");

            try
            {
                var result = await _searchService.AutocompleteAsync(query);

                var searchEvent = new SearchEvent
                {
                    SearchTerm = query,
                    SearchType = "autocomplete",
                    ResultCount = GetAutocompleteCount(result),
                    Metadata = new Dictionary<string, object>
                    {
                        { "isPartialQuery", true },
                        { "queryLength", query.Length }
                    }
                };

                HttpContext.EnrichEvent(searchEvent);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _eventService.PublishSearchEventAsync(searchEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to publish autocomplete event for query: {Query}", query);
                    }
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting autocomplete suggestions for query: {Query}", query);
                return StatusCode(500, "An error occurred while getting suggestions");
            }
        }

        private static int GetResultCount(object result)
        {
            if (result is { } obj)
            {
                // Use reflection to get posts array length
                var posts = obj.GetType().GetProperty("posts")?.GetValue(obj);
                if (posts is IEnumerable enumerable)
                {
                    return enumerable.Cast<object>().Count();
                }
            }
            return 0;
        }

        private static int GetAutocompleteCount(object result)
        {
            if (result is { } obj)
            {
                var tags = obj.GetType().GetProperty("tags")?.GetValue(obj) as IEnumerable;
                var users = obj.GetType().GetProperty("users")?.GetValue(obj) as IEnumerable;

                var tagCount = tags?.Cast<object>().Count() ?? 0;
                var userCount = users?.Cast<object>().Count() ?? 0;

                return tagCount + userCount;
            }
            return 0;
        }
    }
}