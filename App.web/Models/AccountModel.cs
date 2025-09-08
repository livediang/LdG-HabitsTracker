using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.web.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public int FailedLoginAttempts { get; set; } = 0;
        public UserProfile Profile { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }

    public class UserProfile
    {
        [Key]
        public Guid ProfileId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string PhotoUrl { get; set; }
        public string TimeZone { get; set; }
        public string Language { get; set; }
        public string PreferencesJson { get; set; }
        public User User { get; set; }
    }

    public class Role
    {
        [Key]
        public Guid RoleId { get; set; }
        public string Name { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }

    public class UserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public User User { get; set; }
        public Role Role { get; set; }
    }
}
