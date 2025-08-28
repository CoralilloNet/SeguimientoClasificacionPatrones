using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SeguimientoTareas.Web.Data;
using SeguimientoTareas.Web.Models;

namespace SeguimientoTareas.Web.Controllers.Api
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize(Roles = "Admin")]
    public class TaskTemplatesController : ControllerBase
    {
        [HttpGet("templates")]
        public async Task<IActionResult> GetTaskTemplates()
        {
            try
            {
                const string sql = @"
                    SELECT Id, Name, Description, Active, CreatedAt 
                    FROM TaskTemplates 
                    ORDER BY Name";

                var templates = await Db.ExecuteReaderAsync(sql, reader => new TaskTemplate
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Active = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4)
                });

                return Ok(new ApiResponse<List<TaskTemplate>> { Success = true, Data = templates });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPost("templates")]
        public async Task<IActionResult> CreateTaskTemplate([FromBody] TaskTemplate template)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(template.Name))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "El nombre es requerido" });
                }

                const string sql = @"
                    INSERT INTO TaskTemplates (Name, Description, Active) 
                    OUTPUT INSERTED.Id
                    VALUES (@Name, @Description, @Active)";

                var id = await Db.ExecuteScalarAsync<int>(sql,
                    Db.CreateParameter("@Name", template.Name),
                    Db.CreateParameter("@Description", template.Description),
                    Db.CreateParameter("@Active", template.Active));

                template.Id = id;

                return Ok(new ApiResponse<TaskTemplate> { Success = true, Message = "Plantilla de tarea creada exitosamente", Data = template });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPut("templates/{id}")]
        public async Task<IActionResult> UpdateTaskTemplate(int id, [FromBody] TaskTemplate template)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(template.Name))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "El nombre es requerido" });
                }

                const string sql = @"
                    UPDATE TaskTemplates 
                    SET Name = @Name, Description = @Description, Active = @Active
                    WHERE Id = @Id";

                var rowsAffected = await Db.ExecuteNonQueryAsync(sql,
                    Db.CreateParameter("@Id", id),
                    Db.CreateParameter("@Name", template.Name),
                    Db.CreateParameter("@Description", template.Description),
                    Db.CreateParameter("@Active", template.Active));

                if (rowsAffected == 0)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Plantilla de tarea no encontrada" });
                }

                return Ok(new ApiResponse { Success = true, Message = "Plantilla de tarea actualizada exitosamente" });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpDelete("templates/{id}")]
        public async Task<IActionResult> DeleteTaskTemplate(int id)
        {
            try
            {
                const string sql = "DELETE FROM TaskTemplates WHERE Id = @Id";

                var rowsAffected = await Db.ExecuteNonQueryAsync(sql, Db.CreateParameter("@Id", id));

                if (rowsAffected == 0)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Plantilla de tarea no encontrada" });
                }

                return Ok(new ApiResponse { Success = true, Message = "Plantilla de tarea eliminada exitosamente" });
            }
            catch (SqlException ex) when (ex.Number == 547) // Foreign key constraint violation
            {
                return BadRequest(new ApiResponse { Success = false, Message = "No se puede eliminar la plantilla porque tiene asignaciones asociadas" });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("templates/{id}/stages")]
        public async Task<IActionResult> GetTaskStages(int id)
        {
            try
            {
                const string sql = @"
                    SELECT Id, TaskTemplateId, Ordinal, Name, Description, DurationDays 
                    FROM TaskStageTemplates 
                    WHERE TaskTemplateId = @TaskTemplateId
                    ORDER BY Ordinal";

                var stages = await Db.ExecuteReaderAsync(sql, reader => new TaskStageTemplate
                {
                    Id = reader.GetInt32(0),
                    TaskTemplateId = reader.GetInt32(1),
                    Ordinal = reader.GetInt32(2),
                    Name = reader.GetString(3),
                    Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                    DurationDays = reader.GetInt32(5)
                }, Db.CreateParameter("@TaskTemplateId", id));

                return Ok(new ApiResponse<List<TaskStageTemplate>> { Success = true, Data = stages });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPost("templates/{id}/stages")]
        public async Task<IActionResult> CreateTaskStage(int id, [FromBody] TaskStageTemplate stage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(stage.Name))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "El nombre es requerido" });
                }

                if (stage.DurationDays <= 0)
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "La duración debe ser mayor a 0" });
                }

                // Get the next ordinal
                const string getOrdinalSql = @"
                    SELECT ISNULL(MAX(Ordinal), 0) + 1 
                    FROM TaskStageTemplates 
                    WHERE TaskTemplateId = @TaskTemplateId";

                var nextOrdinal = await Db.ExecuteScalarAsync<int>(getOrdinalSql, 
                    Db.CreateParameter("@TaskTemplateId", id));

                const string sql = @"
                    INSERT INTO TaskStageTemplates (TaskTemplateId, Ordinal, Name, Description, DurationDays) 
                    OUTPUT INSERTED.Id
                    VALUES (@TaskTemplateId, @Ordinal, @Name, @Description, @DurationDays)";

                var stageId = await Db.ExecuteScalarAsync<int>(sql,
                    Db.CreateParameter("@TaskTemplateId", id),
                    Db.CreateParameter("@Ordinal", nextOrdinal),
                    Db.CreateParameter("@Name", stage.Name),
                    Db.CreateParameter("@Description", stage.Description),
                    Db.CreateParameter("@DurationDays", stage.DurationDays));

                stage.Id = stageId;
                stage.TaskTemplateId = id;
                stage.Ordinal = nextOrdinal;

                return Ok(new ApiResponse<TaskStageTemplate> { Success = true, Message = "Etapa creada exitosamente", Data = stage });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPut("stages/{id}")]
        public async Task<IActionResult> UpdateTaskStage(int id, [FromBody] TaskStageTemplate stage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(stage.Name))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "El nombre es requerido" });
                }

                if (stage.DurationDays <= 0)
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "La duración debe ser mayor a 0" });
                }

                const string sql = @"
                    UPDATE TaskStageTemplates 
                    SET Name = @Name, Description = @Description, DurationDays = @DurationDays
                    WHERE Id = @Id";

                var rowsAffected = await Db.ExecuteNonQueryAsync(sql,
                    Db.CreateParameter("@Id", id),
                    Db.CreateParameter("@Name", stage.Name),
                    Db.CreateParameter("@Description", stage.Description),
                    Db.CreateParameter("@DurationDays", stage.DurationDays));

                if (rowsAffected == 0)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Etapa no encontrada" });
                }

                return Ok(new ApiResponse { Success = true, Message = "Etapa actualizada exitosamente" });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpDelete("stages/{id}")]
        public async Task<IActionResult> DeleteTaskStage(int id)
        {
            try
            {
                const string sql = "DELETE FROM TaskStageTemplates WHERE Id = @Id";

                var rowsAffected = await Db.ExecuteNonQueryAsync(sql, Db.CreateParameter("@Id", id));

                if (rowsAffected == 0)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Etapa no encontrada" });
                }

                return Ok(new ApiResponse { Success = true, Message = "Etapa eliminada exitosamente" });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }
    }
}