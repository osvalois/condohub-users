using System;
using System.Security.Cryptography;
using AuthService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Services
{
    public class PasswordHashService : IPasswordHashService
    {
        private readonly int _iterations;

        public PasswordHashService(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (!int.TryParse(configuration["Security:PasswordHashingIterations"], out _iterations))
            {
                _iterations = 10000; // Default value if not specified in configuration
            }
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(password, 16, _iterations, HashAlgorithmName.SHA256))
            {
                byte[] salt = deriveBytes.Salt;
                byte[] key = deriveBytes.GetBytes(32);
                return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(key);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            if (string.IsNullOrEmpty(hashedPassword))
            {
                throw new ArgumentException("Hashed password cannot be null or empty.", nameof(hashedPassword));
            }

            string[] parts = hashedPassword.Split(':');
            if (parts.Length != 2)
                return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] key = Convert.FromBase64String(parts[1]);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, _iterations, HashAlgorithmName.SHA256))
            {
                byte[] newKey = deriveBytes.GetBytes(32);
                return CryptographicOperations.FixedTimeEquals(newKey, key);
            }
        }
    }
}