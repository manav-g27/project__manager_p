using System.ComponentModel.DataAnnotations;
namespace ProjectManager.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required, MinLength(3), MaxLength(60)] public string Username { get; set; } = string.Empty;
        [Required] public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        [Required] public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public List<Project> Projects { get; set; } = new();
    }
    public class Project
    {
        public int Id { get; set; }
        [Required, MinLength(3), MaxLength(100)] public string Title { get; set; } = string.Empty;
        [MaxLength(500)] public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        public List<TaskItem> Tasks { get; set; } = new();
    }
    public class TaskItem
    {
        public int Id { get; set; }
        [Required] public string Title { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
    }
}
