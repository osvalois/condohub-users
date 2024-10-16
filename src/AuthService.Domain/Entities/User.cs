using System;

namespace AuthService.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string DepartmentNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Constructor for creating a new user
        public User(string firstName, string lastName, string email, string departmentNumber)
        {
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            DepartmentNumber = departmentNumber;
            CreatedAt = DateTime.UtcNow;
        }

        // Parameterless constructor for EF Core
        public User()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        // Method to update the last login time
        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }
    }
}