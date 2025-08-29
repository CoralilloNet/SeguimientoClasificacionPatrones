using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeguimientoTareas.Web.Data;
using SeguimientoTareas.Web.Models;

namespace SeguimientoTareas.Web.Controllers.Api
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                // Get total tasks stats
                const string statsQuery = @"
                   SELECT
    COUNT(*) AS TotalTasks,
    SUM(CASE WHEN s.IncompleteCount > 0 THEN 1 ELSE 0 END) AS InProgressTasks,
    SUM(CASE WHEN s.IncompleteCount = 0 THEN 1 ELSE 0 END) AS CompletedTasks,
    SUM(CASE WHEN s.OverdueCount > 0 THEN 1 ELSE 0 END) AS OverdueTasks
FROM Assignments a
OUTER APPLY (
    SELECT 
        COUNT(*) AS IncompleteCount,
        SUM(CASE WHEN IsComplete = 0 AND TargetDate < CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS OverdueCount
    FROM AssignmentStages 
    WHERE AssignmentId = a.Id AND IsComplete = 0
) s";

                var stats = await Db.ExecuteReaderSingleAsync(statsQuery, reader => new
                {
                    TotalTasks = reader.GetInt32(0),
                    InProgressTasks = reader.GetInt32(1),
                    CompletedTasks = reader.GetInt32(2),
                    OverdueTasks = reader.GetInt32(3)
                });

                // Get user summaries
                const string userQuery = @"
                    SELECT 
    u.Id,
    u.FullName,
    COUNT(a.Id) AS TotalTasks,
    SUM(CASE WHEN s.IncompleteCount > 0 THEN 1 ELSE 0 END) AS InProgressTasks,
    SUM(CASE WHEN s.IncompleteCount = 0 THEN 1 ELSE 0 END) AS CompletedTasks,
    SUM(CASE WHEN s.OverdueCount > 0 THEN 1 ELSE 0 END) AS OverdueTasks,
    ISNULL(AVG(s.AvgProgress), 0) AS AverageProgress
FROM Users u
LEFT JOIN Assignments a ON u.Id = a.AssignedToUserId
OUTER APPLY (
    SELECT 
        COUNT(*) AS IncompleteCount,
        SUM(CASE WHEN IsComplete = 0 AND TargetDate < CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS OverdueCount,
        AVG(CAST(ProgressPercent AS FLOAT)) AS AvgProgress
    FROM AssignmentStages 
    WHERE AssignmentId = a.Id
) s
WHERE u.IsActive = 1 AND u.IsAdmin = 0
GROUP BY u.Id, u.FullName
ORDER BY u.FullName";

                var userSummaries = await Db.ExecuteReaderAsync(userQuery, reader => new UserTaskSummary
                {
                    UserId = reader.GetInt32(0),
                    UserName = reader.GetString(1),
                    TotalTasks = reader.GetInt32(2),
                    InProgressTasks = reader.GetInt32(3),
                    CompletedTasks = reader.GetInt32(4),
                    OverdueTasks = reader.GetInt32(5),
                    AverageProgress = reader.GetDouble(6)
                });

                var dashboardData = new DashboardStats
                {
                    TotalTasks = stats?.TotalTasks ?? 0,
                    InProgressTasks = stats?.InProgressTasks ?? 0,
                    CompletedTasks = stats?.CompletedTasks ?? 0,
                    OverdueTasks = stats?.OverdueTasks ?? 0,
                    UserSummaries = userSummaries
                };

                return Ok(new ApiResponse<DashboardStats> { Success = true, Data = dashboardData });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }
    }
}