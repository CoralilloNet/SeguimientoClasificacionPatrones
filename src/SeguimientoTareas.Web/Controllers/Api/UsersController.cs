using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SeguimientoTareas.Web.Data;
using SeguimientoTareas.Web.Models;
using System.Text;

namespace SeguimientoTareas.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                const string sql = @"
                    SELECT Id, Email, FullName, IsAdmin, IsActive, CreatedAt 
                    FROM Users 
                    ORDER BY FullName";

                var users = await Db.ExecuteReaderAsync(sql, reader => new
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    FullName = reader.GetString(2),
                    IsAdmin = reader.GetBoolean(3),
                    IsActive = reader.GetBoolean(4),
                    CreatedAt = reader.GetDateTime(5)
                });

                return Ok(new ApiResponse<object> { Success = true, Data = users });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.FullName))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "Email y nombre completo son requeridos" });
                }

                // Generate random password - 12 characters with letters and numbers
                string generatedPassword = GenerateRandomPassword(12);

                const string sql = @"
                    INSERT INTO Users (Email, FullName, Password, IsAdmin, IsActive) 
                    OUTPUT INSERTED.Id
                    VALUES (@Email, @FullName, @Password, @IsAdmin, @IsActive)";

                var id = await Db.ExecuteScalarAsync<int>(sql,
                    Db.CreateParameter("@Email", request.Email),
                    Db.CreateParameter("@FullName", request.FullName),
                    Db.CreateParameter("@Password", generatedPassword),
                    Db.CreateParameter("@IsAdmin", request.IsAdmin),
                    Db.CreateParameter("@IsActive", true));

                var result = new
                {
                    Id = id,
                    Email = request.Email,
                    FullName = request.FullName,
                    GeneratedPassword = generatedPassword // Return the generated password to show to admin
                };

                return Ok(new ApiResponse<object> 
                { 
                    Success = true, 
                    Message = "Usuario creado exitosamente", 
                    Data = result 
                });
            }
            catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Ya existe un usuario con ese email" });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.FullName))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "Email y nombre completo son requeridos" });
                }

                const string sql = @"
                    UPDATE Users 
                    SET Email = @Email, FullName = @FullName, IsAdmin = @IsAdmin, IsActive = @IsActive 
                    WHERE Id = @Id";

                var rowsAffected = await Db.ExecuteNonQueryAsync(sql,
                    Db.CreateParameter("@Id", id),
                    Db.CreateParameter("@Email", request.Email),
                    Db.CreateParameter("@FullName", request.FullName),
                    Db.CreateParameter("@IsAdmin", request.IsAdmin),
                    Db.CreateParameter("@IsActive", request.IsActive));

                if (rowsAffected == 0)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Usuario no encontrado" });
                }

                return Ok(new ApiResponse { Success = true, Message = "Usuario actualizado exitosamente" });
            }
            catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Ya existe un usuario con ese email" });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                const string sql = "DELETE FROM Users WHERE Id = @Id";

                var rowsAffected = await Db.ExecuteNonQueryAsync(sql, Db.CreateParameter("@Id", id));

                if (rowsAffected == 0)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Usuario no encontrado" });
                }

                return Ok(new ApiResponse { Success = true, Message = "Usuario eliminado exitosamente" });
            }
            catch (SqlException ex) when (ex.Number == 547) // Foreign key constraint violation
            {
                return BadRequest(new ApiResponse { Success = false, Message = "No se puede eliminar el usuario porque tiene asignaciones asociadas" });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Generates a random password with letters and numbers
        /// </summary>
        /// <param name="length">Length of the password</param>
        /// <returns>Generated password</returns>
        private static string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var password = new StringBuilder();
            
            for (int i = 0; i < length; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }
            
            return password.ToString();
        }
    }

    public class CreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
    }

    public class UpdateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
    }
}