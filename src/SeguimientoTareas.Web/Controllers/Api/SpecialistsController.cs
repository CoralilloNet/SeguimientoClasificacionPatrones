using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SeguimientoTareas.Web.Data;
using SeguimientoTareas.Web.Models;

namespace SeguimientoTareas.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SpecialistsController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetSpecialists()
        {
            try
            {
                const string sql = @"
                    SELECT Id, Name, Email, Active, CreatedAt 
                    FROM Specialists 
                    ORDER BY Name";

                var specialists = await Db.ExecuteReaderAsync(sql, reader => new Specialist
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Active = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4)
                });

                return Ok(new ApiResponse<List<Specialist>> { Success = true, Data = specialists });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSpecialist([FromBody] Specialist specialist)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(specialist.Name))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "El nombre es requerido" });
                }

                const string sql = @"
                    INSERT INTO Specialists (Name, Email, Active) 
                    OUTPUT INSERTED.Id
                    VALUES (@Name, @Email, @Active)";

                var id = await Db.ExecuteScalarAsync<int>(sql,
                    Db.CreateParameter("@Name", specialist.Name),
                    Db.CreateParameter("@Email", specialist.Email),
                    Db.CreateParameter("@Active", specialist.Active));

                specialist.Id = id;

                return Ok(new ApiResponse<Specialist> { Success = true, Message = "Especialista creado exitosamente", Data = specialist });
            }
            catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Ya existe un especialista con ese nombre" });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSpecialist(int id, [FromBody] Specialist specialist)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(specialist.Name))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "El nombre es requerido" });
                }

                const string sql = @"
                    UPDATE Specialists 
                    SET Name = @Name, Email = @Email, Active = @Active
                    WHERE Id = @Id";

                var rowsAffected = await Db.ExecuteNonQueryAsync(sql,
                    Db.CreateParameter("@Id", id),
                    Db.CreateParameter("@Name", specialist.Name),
                    Db.CreateParameter("@Email", specialist.Email),
                    Db.CreateParameter("@Active", specialist.Active));

                if (rowsAffected == 0)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Especialista no encontrado" });
                }

                return Ok(new ApiResponse { Success = true, Message = "Especialista actualizado exitosamente" });
            }
            catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Ya existe un especialista con ese nombre" });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpecialist(int id)
        {
            try
            {
                const string sql = "DELETE FROM Specialists WHERE Id = @Id";

                var rowsAffected = await Db.ExecuteNonQueryAsync(sql, Db.CreateParameter("@Id", id));

                if (rowsAffected == 0)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Especialista no encontrado" });
                }

                return Ok(new ApiResponse { Success = true, Message = "Especialista eliminado exitosamente" });
            }
            catch (SqlException ex) when (ex.Number == 547) // Foreign key constraint violation
            {
                return BadRequest(new ApiResponse { Success = false, Message = "No se puede eliminar el especialista porque tiene asignaciones asociadas" });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }
    }
}