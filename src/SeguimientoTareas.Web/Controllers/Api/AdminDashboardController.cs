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
                        COUNT(*) as TotalTasks,
                        SUM(CASE WHEN EXISTS(SELECT 1 FROM AssignmentStages WHERE AssignmentId = a.Id AND IsComplete = 0) THEN 1 ELSE 0 END) as InProgressTasks,
                        SUM(CASE WHEN NOT EXISTS(SELECT 1 FROM AssignmentStages WHERE AssignmentId = a.Id AND IsComplete = 0) THEN 1 ELSE 0 END) as CompletedTasks,
                        SUM(CASE WHEN EXISTS(SELECT 1 FROM AssignmentStages WHERE AssignmentId = a.Id AND IsComplete = 0 AND TargetDate < CAST(GETDATE() AS DATE)) THEN 1 ELSE 0 END) as OverdueTasks
                    FROM Assignments a";

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
                        COUNT(a.Id) as TotalTasks,
                        SUM(CASE WHEN EXISTS(SELECT 1 FROM AssignmentStages WHERE AssignmentId = a.Id AND IsComplete = 0) THEN 1 ELSE 0 END) as InProgressTasks,
                        SUM(CASE WHEN NOT EXISTS(SELECT 1 FROM AssignmentStages WHERE AssignmentId = a.Id AND IsComplete = 0) THEN 1 ELSE 0 END) as CompletedTasks,
                        SUM(CASE WHEN EXISTS(SELECT 1 FROM AssignmentStages WHERE AssignmentId = a.Id AND IsComplete = 0 AND TargetDate < CAST(GETDATE() AS DATE)) THEN 1 ELSE 0 END) as OverdueTasks,
                        ISNULL(AVG(CAST((SELECT AVG(CAST(ProgressPercent AS FLOAT)) FROM AssignmentStages WHERE AssignmentId = a.Id) AS FLOAT)), 0) as AverageProgress
                    FROM Users u
                    LEFT JOIN Assignments a ON u.Id = a.AssignedToUserId
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