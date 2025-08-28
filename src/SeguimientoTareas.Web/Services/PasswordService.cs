namespace SeguimientoTareas.Web.Services
{
    public interface IPasswordService
    {
        string StorePassword(string password);
        bool VerifyPassword(string password, string storedPassword);
    }

    public class PasswordService : IPasswordService
    {
        public string StorePassword(string password)
        {
            // Store password as plain text - no encryption
            return password;
        }

        public bool VerifyPassword(string password, string storedPassword)
        {
            // Direct string comparison - no hashing
            return string.Equals(password, storedPassword, StringComparison.Ordinal);
        }
    }
}