using Microsoft.EntityFrameworkCore;
using RealtimeWhiteboard.Models;

namespace RealtimeWhiteboard.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<WhiteboardSession> WhiteboardSessions => Set<WhiteboardSession>();
    public DbSet<DrawingLog> DrawingLogs => Set<DrawingLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WhiteboardSession>()
            .HasIndex(s => s.Name);

        modelBuilder.Entity<DrawingLog>()
            .HasOne(log => log.WhiteboardSession)
            .WithMany(session => session.DrawingLogs)
            .HasForeignKey(log => log.WhiteboardSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DrawingLog>()
            .HasOne(log => log.User)
            .WithMany()
            .HasForeignKey(log => log.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
