using Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Core;

public class TodoDbContext : IdentityDbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

    public DbSet<Todo> Todos { get; set; }
    public DbSet<TodoTask> TodoTasks { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<TodoUser> TodoUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        //builder.Entity<Todo>()
        //       .HasOne<TodoUser>()
        //       .WithMany()
        //       .HasForeignKey(t => t.OwnerId)
        //       .HasPrincipalKey(u => u.UserName);

        base.OnModelCreating(builder);
    }
}