using Microsoft.EntityFrameworkCore;
using ProjectManager.Api.Models;
namespace ProjectManager.Api.Data
{
    public class AppDb : DbContext
    {
        public AppDb(DbContextOptions<AppDb> opt) : base(opt) {}
        public DbSet<User> Users => Set<User>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
    }
}
