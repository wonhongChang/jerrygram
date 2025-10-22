using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/search/popular")]
    public class PopularSearchController : ControllerBase
    {
        private readonly IPopularSearchService _popularSearchService;

        [HttpGet]
        public async Task<ActionResult<List<PopularSearchDto>>> GetPopularSearches(
            [FromQuery] int limit = 10,
            [FromQuery] int hours = 24)
        {
            if (limit <= 0 || limit > 50) return BadRequest("Limit must be between 1 and 50");
            if (hours <= 0 || hours > 168) return BadRequest("Hours must be between 1 and 168");

            var timeWindow = TimeSpan.FromHours(hours);
            var results = await _popularSearchService.GetPopularSearchesAsync(limit, timeWindow);
            return Ok(results);
        }

        [HttpGet("trending")]
        public async Task<ActionResult<List<PopularSearchDto>>> GetTrendingSearches([FromQuery] int limit = 5)
        {
            if (limit <= 0 || limit > 20) return BadRequest("Limit must be between 1 and 20");

            var results = await _popularSearchService.GetTrendingSearchesAsync(limit);
            return Ok(results);
        }
    }
}
