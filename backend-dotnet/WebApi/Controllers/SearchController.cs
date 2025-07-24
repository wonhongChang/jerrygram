using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { error = "Query is required." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _searchService.SearchAsync(query, userId);

            return Ok(result);
        }

        [HttpGet("autocomplete")]
        [AllowAnonymous]
        public async Task<IActionResult> Autocomplete([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query is required.");

            var result = await _searchService.AutocompleteAsync(query);
            return Ok(result);
        }
    }
}