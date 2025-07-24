using Persistence.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(AppDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            var healthReport = new HealthReport
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = GetAssemblyVersion(),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            };

            // Check database connectivity
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                healthReport.Dependencies.Add("Database", canConnect ? "Healthy" : "Unhealthy");
            }
            catch (Exception ex)
            {
                healthReport.Dependencies.Add("Database", "Unhealthy");
                healthReport.Status = "Degraded";
                _logger.LogError(ex, "Database health check failed");
            }

            // System information
            var process = Process.GetCurrentProcess();
            healthReport.SystemInfo = new SystemInfo
            {
                MachineName = Environment.MachineName,
                ProcessId = process.Id,
                WorkingSet = process.WorkingSet64,
                ProcessorTime = process.TotalProcessorTime,
                ThreadCount = process.Threads.Count,
                HandleCount = process.HandleCount,
                Uptime = DateTime.UtcNow - process.StartTime,
                GCInfo = new GCInfo
                {
                    Gen0Collections = GC.CollectionCount(0),
                    Gen1Collections = GC.CollectionCount(1),
                    Gen2Collections = GC.CollectionCount(2),
                    TotalMemory = GC.GetTotalMemory(false)
                }
            };

            var statusCode = healthReport.Status == "Healthy" ? 200 : 503;
            return StatusCode(statusCode, healthReport);
        }

        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailedHealth()
        {
            var detailedReport = new DetailedHealthReport
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = GetAssemblyVersion(),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            };

            // Database checks
            var dbChecks = new List<HealthCheck>();
            
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var canConnect = await _context.Database.CanConnectAsync();
                stopwatch.Stop();
                
                dbChecks.Add(new HealthCheck
                {
                    Name = "Database Connection",
                    Status = canConnect ? "Healthy" : "Unhealthy",
                    ResponseTime = stopwatch.Elapsed,
                    Description = "PostgreSQL database connectivity"
                });

                if (canConnect)
                {
                    stopwatch.Restart();
                    var userCount = await _context.Users.CountAsync();
                    stopwatch.Stop();
                    
                    dbChecks.Add(new HealthCheck
                    {
                        Name = "Database Query",
                        Status = "Healthy",
                        ResponseTime = stopwatch.Elapsed,
                        Description = $"User count query returned {userCount} users"
                    });
                }
            }
            catch (Exception ex)
            {
                dbChecks.Add(new HealthCheck
                {
                    Name = "Database Connection",
                    Status = "Unhealthy",
                    Description = ex.Message
                });
                detailedReport.Status = "Unhealthy";
            }

            detailedReport.HealthChecks.AddRange(dbChecks);

            var statusCode = detailedReport.Status == "Healthy" ? 200 : 503;
            return StatusCode(statusCode, detailedReport);
        }

        private static string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly()
                .GetName().Version?.ToString() ?? "Unknown";
        }
    }

    public class HealthReport
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public Dictionary<string, string> Dependencies { get; set; } = new();
        public SystemInfo SystemInfo { get; set; } = new();
    }

    public class DetailedHealthReport : HealthReport
    {
        public List<HealthCheck> HealthChecks { get; set; } = new();
    }

    public class HealthCheck
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public TimeSpan? ResponseTime { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class SystemInfo
    {
        public string MachineName { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public long WorkingSet { get; set; }
        public TimeSpan ProcessorTime { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public TimeSpan Uptime { get; set; }
        public GCInfo GCInfo { get; set; } = new();
    }

    public class GCInfo
    {
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
        public long TotalMemory { get; set; }
    }
}