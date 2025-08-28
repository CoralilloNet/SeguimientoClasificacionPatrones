using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeguimientoTareas.Web.Data;
using SeguimientoTareas.Web.Models;
using System.Security.Claims;

namespace SeguimientoTareas.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public StagesController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("{id}/progress")]
        public async Task<IActionResult> UpdateStageProgress(int id, [FromBody] UpdateProgressRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Verify user has access to this stage
                const string checkAccessSql = @"
                    SELECT COUNT(*) FROM AssignmentStages ast
                    INNER JOIN Assignments a ON ast.AssignmentId = a.Id
                    WHERE ast.Id = @StageId AND a.AssignedToUserId = @UserId";

                var hasAccess = await Db.ExecuteScalarAsync<int>(checkAccessSql,
                    Db.CreateParameter("@StageId", id),
                    Db.CreateParameter("@UserId", userId));

                if (hasAccess == 0)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Etapa no encontrada" });
                }

                // Update progress
                const string updateSql = @"
                    UPDATE AssignmentStages 
                    SET ProgressPercent = @ProgressPercent,
                        IsComplete = CASE WHEN @ProgressPercent >= 100 THEN 1 ELSE 0 END,
                        CompletedAt = CASE WHEN @ProgressPercent >= 100 THEN SYSUTCDATETIME() ELSE NULL END
                    WHERE Id = @StageId";

                var rowsAffected = await Db.ExecuteNonQueryAsync(updateSql,
                    Db.CreateParameter("@StageId", id),
                    Db.CreateParameter("@ProgressPercent", request.ProgressPercent));

                if (rowsAffected == 0)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Etapa no encontrada" });
                }

                return Ok(new ApiResponse { Success = true, Message = "Progreso actualizado exitosamente" });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPost("{id}/evidence")]
        public async Task<IActionResult> UploadEvidence(int id, [FromForm] UploadEvidenceRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Verify user has access to this stage
                const string checkAccessSql = @"
                    SELECT a.Id as AssignmentId FROM AssignmentStages ast
                    INNER JOIN Assignments a ON ast.AssignmentId = a.Id
                    WHERE ast.Id = @StageId AND a.AssignedToUserId = @UserId";

                var assignmentId = await Db.ExecuteScalarAsync<int?>(checkAccessSql,
                    Db.CreateParameter("@StageId", id),
                    Db.CreateParameter("@UserId", userId));

                if (!assignmentId.HasValue)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Etapa no encontrada" });
                }

                // Validate file
                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "No se ha seleccionado ningún archivo" });
                }

                // Check file size (20MB max)
                if (request.File.Length > 20 * 1024 * 1024)
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "El archivo es demasiado grande (máx. 20MB)" });
                }

                // Check file type
                var allowedTypes = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".docx", ".xlsx" };
                var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
                
                if (!allowedTypes.Contains(fileExtension))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "Tipo de archivo no permitido" });
                }

                // Create directory structure
                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", assignmentId.Value.ToString(), id.ToString());
                Directory.CreateDirectory(uploadsDir);

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                // Save evidence record
                const string insertSql = @"
                    INSERT INTO StageEvidences (AssignmentStageId, FileName, FilePath, Notes, UploadedByUserId)
                    VALUES (@AssignmentStageId, @FileName, @FilePath, @Notes, @UploadedByUserId)";

                var relativePath = $"/uploads/{assignmentId}/{id}/{fileName}";

                await Db.ExecuteNonQueryAsync(insertSql,
                    Db.CreateParameter("@AssignmentStageId", id),
                    Db.CreateParameter("@FileName", request.File.FileName),
                    Db.CreateParameter("@FilePath", relativePath),
                    Db.CreateParameter("@Notes", request.Notes),
                    Db.CreateParameter("@UploadedByUserId", userId));

                return Ok(new ApiResponse { Success = true, Message = "Evidencia subida exitosamente" });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }
    }

    public class UpdateProgressRequest
    {
        public int ProgressPercent { get; set; }
        public string? Notes { get; set; }
    }

    public class UploadEvidenceRequest
    {
        public IFormFile File { get; set; } = null!;
        public string? Notes { get; set; }
    }
}