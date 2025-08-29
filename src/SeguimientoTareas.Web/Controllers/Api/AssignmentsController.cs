using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SeguimientoTareas.Web.Data;
using SeguimientoTareas.Web.Models;
using System.Security.Claims;

namespace SeguimientoTareas.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssignmentsController : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentRequest request)
        {
            try
            {
                if (request.TaskTemplateId <= 0 || request.AssignedToUserId <= 0)
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "Datos de asignación inválidos" });
                }

                var assignedByUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                using var connection = Db.GetConnection();
                await connection.OpenAsync();
                
                using var transaction = connection.BeginTransaction();
                
                try
                {
                    // Create assignment - SpecialistId removed
                    const string insertAssignmentSql = @"
                        INSERT INTO Assignments (TaskTemplateId, Title, Description, AssignedToUserId, AssignedByUserId, StartDate, DueDate)
                        OUTPUT INSERTED.Id
                        VALUES (@TaskTemplateId, @Title, @Description, @AssignedToUserId, @AssignedByUserId, @StartDate, @DueDate)";

                    using var assignmentCmd = new SqlCommand(insertAssignmentSql, connection, transaction);
                    assignmentCmd.Parameters.AddRange(new[]
                    {
                        Db.CreateParameter("@TaskTemplateId", request.TaskTemplateId),
                        Db.CreateParameter("@Title", request.Title),
                        Db.CreateParameter("@Description", request.Description),
                        Db.CreateParameter("@AssignedToUserId", request.AssignedToUserId),
                        Db.CreateParameter("@AssignedByUserId", assignedByUserId),
                        // SpecialistId parameter removed
                        Db.CreateParameter("@StartDate", request.StartDate),
                        Db.CreateParameter("@DueDate", request.DueDate)
                    });

                    var assignmentId = (int)(await assignmentCmd.ExecuteScalarAsync() ?? 0);

                    // Copy stages from template
                    const string getStagesSql = @"
                        SELECT Ordinal, Name, Description, DurationDays 
                        FROM TaskStageTemplates 
                        WHERE TaskTemplateId = @TaskTemplateId
                        ORDER BY Ordinal";

                    using var stagesCmd = new SqlCommand(getStagesSql, connection, transaction);
                    stagesCmd.Parameters.Add(Db.CreateParameter("@TaskTemplateId", request.TaskTemplateId));

                    using var reader = await stagesCmd.ExecuteReaderAsync();
                    var stages = new List<(int Ordinal, string Name, string Description, int DurationDays)>();
                    
                    while (await reader.ReadAsync())
                    {
                        stages.Add((
                            reader.GetInt32(0), // Ordinal
                            reader.GetString(1), // Name
                            reader.IsDBNull(2) ? null : reader.GetString(2), // Description
                            reader.GetInt32(3) // DurationDays
                        ));
                    }
                    reader.Close();

                    // Insert assignment stages
                    var currentDate = request.StartDate;
                    foreach (var stage in stages)
                    {
                        var targetDate = currentDate.AddDays(stage.DurationDays);

                        const string insertStageSql = @"
                            INSERT INTO AssignmentStages (AssignmentId, Ordinal, Name, Description, DurationDays, StartDate, TargetDate, ProgressPercent, IsComplete)
                            VALUES (@AssignmentId, @Ordinal, @Name, @Description, @DurationDays, @StartDate, @TargetDate, 0, 0)";

                        using var stageCmd = new SqlCommand(insertStageSql, connection, transaction);
                        stageCmd.Parameters.AddRange(new[]
                        {
                            Db.CreateParameter("@AssignmentId", assignmentId),
                            Db.CreateParameter("@Ordinal", stage.Ordinal),
                            Db.CreateParameter("@Name", stage.Name),
                            Db.CreateParameter("@Description", stage.Description),
                            Db.CreateParameter("@DurationDays", stage.DurationDays),
                            Db.CreateParameter("@StartDate", currentDate),
                            Db.CreateParameter("@TargetDate", targetDate)
                        });

                        await stageCmd.ExecuteNonQueryAsync();
                        currentDate = targetDate;
                    }

                    transaction.Commit();

                    return Ok(new ApiResponse<int> 
                    { 
                        Success = true, 
                        Message = "Asignación creada exitosamente", 
                        Data = assignmentId 
                    });
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        // Admin endpoint to get all assignments - added for admin assignment management
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAssignments()
        {
            try
            {
                const string sql = @"
                    SELECT 
                        a.Id, a.Title, a.Description, a.StartDate, a.DueDate, a.CreatedAt,
                        tt.Name as TaskTemplateName,
                        u1.FullName as AssignedToUserName,
                        u2.FullName as AssignedByUserName
                    FROM Assignments a
                    INNER JOIN TaskTemplates tt ON a.TaskTemplateId = tt.Id
                    INNER JOIN Users u1 ON a.AssignedToUserId = u1.Id
                    INNER JOIN Users u2 ON a.AssignedByUserId = u2.Id
                    ORDER BY a.CreatedAt DESC";

                var assignments = await Db.ExecuteReaderAsync(sql, reader => new
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    StartDate = reader.GetDateTime(3),
                    DueDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                    CreatedAt = reader.GetDateTime(5),
                    TaskTemplateName = reader.GetString(6),
                    // SpecialistName removed
                    AssignedToUserName = reader.GetString(7),
                    AssignedByUserName = reader.GetString(8)
                });

                return Ok(new ApiResponse<object> { Success = true, Data = assignments });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyAssignments()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                const string sql = @"
                    SELECT 
                        a.Id, a.Title, a.Description, a.StartDate, a.DueDate, a.CreatedAt,
                        tt.Name as TaskTemplateName,
                        u.FullName as AssignedByUserName
                    FROM Assignments a
                    INNER JOIN TaskTemplates tt ON a.TaskTemplateId = tt.Id
                    INNER JOIN Users u ON a.AssignedByUserId = u.Id
                    WHERE a.AssignedToUserId = @UserId
                    ORDER BY a.CreatedAt DESC";

                var assignments = await Db.ExecuteReaderAsync(sql, reader => new
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    StartDate = reader.GetDateTime(3),
                    DueDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                    CreatedAt = reader.GetDateTime(5),
                    TaskTemplateName = reader.GetString(6),
                    // SpecialistName removed
                    AssignedByUserName = reader.GetString(7)
                }, Db.CreateParameter("@UserId", userId));

                return Ok(new ApiResponse<object> { Success = true, Data = assignments });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssignmentDetails(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var isAdmin = User.IsInRole("Admin");

                // Check if user has access to this assignment
                const string checkAccessSql = @"
                    SELECT COUNT(*) FROM Assignments 
                    WHERE Id = @Id AND (@IsAdmin = 1 OR AssignedToUserId = @UserId)";

                var hasAccess = await Db.ExecuteScalarAsync<int>(checkAccessSql,
                    Db.CreateParameter("@Id", id),
                    Db.CreateParameter("@IsAdmin", isAdmin ? 1 : 0),
                    Db.CreateParameter("@UserId", userId));

                if (hasAccess == 0)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Asignación no encontrada" });
                }

                // Get assignment details - specialist removed
                const string assignmentSql = @"
                    SELECT 
                        a.Id, a.Title, a.Description, a.StartDate, a.DueDate, a.CreatedAt,
                        tt.Name as TaskTemplateName,
                        u1.FullName as AssignedToUserName,
                        u2.FullName as AssignedByUserName
                    FROM Assignments a
                    INNER JOIN TaskTemplates tt ON a.TaskTemplateId = tt.Id
                    INNER JOIN Users u1 ON a.AssignedToUserId = u1.Id
                    INNER JOIN Users u2 ON a.AssignedByUserId = u2.Id
                    WHERE a.Id = @Id";

                var assignment = await Db.ExecuteReaderSingleAsync(assignmentSql, reader => new
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    StartDate = reader.GetDateTime(3),
                    DueDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                    CreatedAt = reader.GetDateTime(5),
                    TaskTemplateName = reader.GetString(6),
                    // SpecialistName removed
                    AssignedToUserName = reader.GetString(7),
                    AssignedByUserName = reader.GetString(8)
                }, Db.CreateParameter("@Id", id));

                // Get stages
                const string stagesSql = @"
                    SELECT 
                        Id, Ordinal, Name, Description, DurationDays, StartDate, TargetDate, 
                        ProgressPercent, IsComplete, CompletedAt
                    FROM AssignmentStages
                    WHERE AssignmentId = @AssignmentId
                    ORDER BY Ordinal";

                var stages = await Db.ExecuteReaderAsync(stagesSql, reader => new
                {
                    Id = reader.GetInt32(0),
                    Ordinal = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    DurationDays = reader.GetInt32(4),
                    StartDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                    TargetDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                    ProgressPercent = reader.GetInt32(7),
                    IsComplete = reader.GetBoolean(8),
                    CompletedAt = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
                    IsOverdue = !reader.GetBoolean(8) && !reader.IsDBNull(6) && reader.GetDateTime(6) < DateTime.Today
                }, Db.CreateParameter("@AssignmentId", id));

                // Calculate overall progress
                var overallProgress = stages.Any() ? stages.Average(s => s.ProgressPercent) : 0;

                var result = new
                {
                    Assignment = assignment,
                    Stages = stages,
                    OverallProgress = Math.Round(overallProgress, 1)
                };

                return Ok(new ApiResponse<object> { Success = true, Data = result });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        // Admin endpoint to delete assignments - added for admin assignment management
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            try
            {
                const string sql = "DELETE FROM Assignments WHERE Id = @Id";

                var rowsAffected = await Db.ExecuteNonQueryAsync(sql, Db.CreateParameter("@Id", id));

                if (rowsAffected == 0)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Asignación no encontrada" });
                }

                return Ok(new ApiResponse { Success = true, Message = "Asignación eliminada exitosamente" });
            }
            catch (SqlException ex) when (ex.Number == 547) // Foreign key constraint violation
            {
                return BadRequest(new ApiResponse { Success = false, Message = "No se puede eliminar la asignación porque tiene datos asociados" });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }
    }

    public class CreateAssignmentRequest
    {
        public int TaskTemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int AssignedToUserId { get; set; }
        // SpecialistId removed - assignments now directly use Users
        public DateTime StartDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
}