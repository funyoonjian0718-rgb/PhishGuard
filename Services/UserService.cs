using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Data;
using PhishGuard.Models;

namespace PhishGuard.Services
{
    public class UserService
    {
        private readonly PhishGuardDbContext _dbContext;

        public UserService(PhishGuardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<object> RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return new
                {
                    Success = false,
                    Message = "Full name, email and password are required."
                };
            }

            if (request.Password.Length < 8)
            {
                return new
                {
                    Success = false,
                    Message = "Password must be at least 8 characters long."
                };
            }

            string email = request.Email.Trim().ToLower();

            bool emailExists = await _dbContext.Users.AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                return new
                {
                    Success = false,
                    Message = "This email is already registered."
                };
            }

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = HashPassword(request.Password),
                Role = "User",
                CreatedAt = DateTime.Now
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return new
            {
                Success = true,
                Message = "Registration successful.",
                User = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Role
                }
            };
        }

        public async Task<object> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return new
                {
                    Success = false,
                    Message = "Email and password are required."
                };
            }

            string email = request.Email.Trim().ToLower();

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            bool passwordValid = VerifyPassword(request.Password, user.PasswordHash);

            if (!passwordValid)
            {
                return new
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            return new
            {
                Success = true,
                Message = "Login successful.",
                User = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Role
                }
            };
        }

        private string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                100000,
                HashAlgorithmName.SHA256
            );

            byte[] hash = pbkdf2.GetBytes(32);

            return $"100000.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                string[] parts = storedHash.Split('.');

                if (parts.Length != 3)
                {
                    return false;
                }

                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] originalHash = Convert.FromBase64String(parts[2]);

                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256
                );

                byte[] newHash = pbkdf2.GetBytes(32);

                return CryptographicOperations.FixedTimeEquals(originalHash, newHash);
            }
            catch
            {
                return false;
            }
        }
    }
}