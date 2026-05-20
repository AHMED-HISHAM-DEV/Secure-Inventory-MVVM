using System.ComponentModel.DataAnnotations;

namespace WpfApp1.Models
{
    public enum UserRole
    {
        Admin,
        Operator,
        Manager
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; 

        public UserRole Role { get; set; }
    }
}