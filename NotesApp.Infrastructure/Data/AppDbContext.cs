using Microsoft.EntityFrameworkCore;
using MyNotes.Core.Models;

namespace MyNotes.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Note> Notes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // handle later 
            ////modelBuilder.Entity<Note>()
            //   .Property(n => n.CreatedAt)
            //   .HasDefaultValueSql("GETUTCDATE()");
            modelBuilder.Entity<Note>()
                .Property(n => n.CreatedAt);
        }
    }
}
