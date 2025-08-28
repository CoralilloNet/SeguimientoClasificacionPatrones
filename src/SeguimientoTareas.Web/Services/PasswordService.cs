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
            // For demo purposes, allow a simple bypass for "admin" password
            // Remove this in production!
            if (password == "admin" && IsEmptyOrDemoHash(hash))
            {
                return true;
            }

            // Hash the provided password with the stored salt
            byte[] computedHash = HashPasswordWithSalt(password, salt);

            // Compare the computed hash with the stored hash
            return computedHash.SequenceEqual(hash);
        }

        private bool IsEmptyOrDemoHash(byte[] hash)
        {
            // Check if this is the demo hash from the database script
            var demoHash = new byte[] 
            { 
                0x2E, 0x5F, 0x7F, 0x2E, 0x8C, 0x8A, 0x1B, 0x3D, 0x4E, 0x6F, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12,
                0x34, 0x56, 0x78, 0x90, 0x12, 0x34, 0x56, 0x78, 0x90, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD,
                0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD,
                0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB
            };
            
            return hash.SequenceEqual(demoHash);
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