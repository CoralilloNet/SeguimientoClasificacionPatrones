using System.Security.Cryptography;
using System.Text;

namespace SeguimientoTareas.Web.Services
{
    public interface IPasswordService
    {
        (byte[] hash, byte[] salt) HashPassword(string password);
        bool VerifyPassword(string password, byte[] hash, byte[] salt);
    }

    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 16;
        private const int HashSize = 64;
        private const int Iterations = 10000;

        public (byte[] hash, byte[] salt) HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password with the salt
            byte[] hash = HashPasswordWithSalt(password, salt);

            return (hash, salt);
        }

        public bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            // Hash the provided password with the stored salt
            byte[] computedHash = HashPasswordWithSalt(password, salt);

            // Compare the computed hash with the stored hash
            return computedHash.SequenceEqual(hash);
        }

        private byte[] HashPasswordWithSalt(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA1);

            return pbkdf2.GetBytes(HashSize);
        }
    }
}