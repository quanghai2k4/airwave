using AirWave.Shared.Data;
using AirWave.Shared.Helpers;
using AirWave.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AirWave.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class AqiController : ControllerBase
{
    private readonly AqiDbContext _context;
    private readonly ILogger<AqiController> _logger;

    public AqiController(AqiDbContext context, ILogger<AqiController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var data = await _context.AqiRecords
            .OrderByDescending(x => x.Timestamp)
            .Take(100)
            .Select(x => new
            {
                x.Id,
                x.AqiValue,
                Timestamp = x.Timestamp,
                Category = AqiHelper.GetAqiCategory(x.AqiValue),
                Color = AqiHelper.GetAqiColor(x.AqiValue)
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("latest")]
    [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { })]
    public async Task<ActionResult<object>> GetLatest()
    {
        var latest = await _context.AqiRecords
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync();

        if (latest == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            latest.Id,
            latest.AqiValue,
            Timestamp = latest.Timestamp,
            Category = AqiHelper.GetAqiCategory(latest.AqiValue),
            Color = AqiHelper.GetAqiColor(latest.AqiValue)
        });
    }

    [HttpGet("filter")]
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "StartDate", "EndDate" })]
    public async Task<ActionResult<IEnumerable<object>>> GetFiltered([FromQuery] AqiFilterRequest request)
    {
        // Manual validation check (FluentValidation auto-validation is configured in Program.cs)
        if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
        {
            return BadRequest(new { error = "Start date must be before or equal to end date" });
        }

        if (request.StartDate.HasValue && request.StartDate > DateTime.UtcNow)
        {
            return BadRequest(new { error = "Start date cannot be in the future" });
        }

        if (request.EndDate.HasValue && request.EndDate > DateTime.UtcNow)
        {
            return BadRequest(new { error = "End date cannot be in the future" });
        }

        var query = _context.AqiRecords.AsQueryable();

        if (request.StartDate.HasValue)
        {
            var startDateTime = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Unspecified);
            query = query.Where(x => x.Timestamp >= startDateTime);
        }

        if (request.EndDate.HasValue)
        {
            var endDateTime = DateTime.SpecifyKind(request.EndDate.Value.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Unspecified);
            query = query.Where(x => x.Timestamp <= endDateTime);
        }

        var data = await query
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new
            {
                x.Id,
                x.AqiValue,
                Timestamp = x.Timestamp,
                Category = AqiHelper.GetAqiCategory(x.AqiValue),
                Color = AqiHelper.GetAqiColor(x.AqiValue)
            })
            .ToListAsync();

        return Ok(data);
    }
}
