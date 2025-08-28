using Microsoft.Data.SqlClient;
using System.Data;

namespace SeguimientoTareas.Web.Data
{
    public static class Db
    {
        private static string? _connectionString;

        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static SqlConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Database connection string not initialized");
            
            return new SqlConnection(_connectionString);
        }

        public static async Task<T?> ExecuteScalarAsync<T>(string sql, params SqlParameter[] parameters)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(sql, connection);
            
            if (parameters?.Length > 0)
                command.Parameters.AddRange(parameters);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            
            return result == null || result == DBNull.Value ? default(T) : (T)result;
        }

        public static async Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(sql, connection);
            
            if (parameters?.Length > 0)
                command.Parameters.AddRange(parameters);

            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync();
        }

        public static async Task<List<T>> ExecuteReaderAsync<T>(string sql, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters)
        {
            var results = new List<T>();
            
            using var connection = GetConnection();
            using var command = new SqlCommand(sql, connection);
            
            if (parameters?.Length > 0)
                command.Parameters.AddRange(parameters);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(mapper(reader));
            }
            
            return results;
        }

        public static async Task<T?> ExecuteReaderSingleAsync<T>(string sql, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(sql, connection);
            
            if (parameters?.Length > 0)
                command.Parameters.AddRange(parameters);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return mapper(reader);
            }
            
            return default(T);
        }

        public static SqlParameter CreateParameter(string name, object? value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static SqlParameter CreateParameter(string name, SqlDbType dbType, object? value)
        {
            var parameter = new SqlParameter(name, dbType);
            parameter.Value = value ?? DBNull.Value;
            return parameter;
        }
    }
}