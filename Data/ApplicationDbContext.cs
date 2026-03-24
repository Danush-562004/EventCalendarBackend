using EventCalendarAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EventCalendarAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
                entity.Property(u => u.Role).HasDefaultValue("User");
            });

            // Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.ColorCode).HasDefaultValue("#3498db").HasMaxLength(10);
            });

            // Venue
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Name).IsRequired().HasMaxLength(200);
                entity.Property(v => v.Address).IsRequired().HasMaxLength(500);
            });

            // Event
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Privacy).HasConversion<string>();
                entity.Property(e => e.Recurrence).HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Events)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Events)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Venue)
                    .WithMany(v => v.Events)
                    .HasForeignKey(e => e.VenueId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Ticket
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.TicketNumber).IsUnique();
                entity.Property(t => t.TicketNumber).IsRequired().HasMaxLength(50);
                entity.Property(t => t.Price).HasColumnType("decimal(18,2)");
                entity.Property(t => t.Type).HasConversion<string>();
                entity.Property(t => t.Status).HasConversion<string>();

                entity.HasOne(t => t.Event)
                    .WithMany(e => e.Tickets)
                    .HasForeignKey(t => t.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.User)
                    .WithMany(u => u.Tickets)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Status).HasConversion<string>();
                entity.Property(p => p.Method).HasConversion<string>();

                entity.HasOne(p => p.Ticket)
                    .WithMany(t => t.Payments)
                    .HasForeignKey(p => p.TicketId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Reminder
            modelBuilder.Entity<Reminder>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Title).IsRequired().HasMaxLength(300);
                entity.Property(r => r.Type).HasConversion<string>();

                entity.HasOne(r => r.Event)
                    .WithMany(e => e.Reminders)
                    .HasForeignKey(r => r.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reminders)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AuditLog
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Action).IsRequired().HasMaxLength(100);
                entity.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(a => a.UserName).HasMaxLength(200);
                entity.Property(a => a.IpAddress).HasMaxLength(50);
            });

            // Seed default categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Personal", ColorCode = "#3498db", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Category { Id = 2, Name = "Work", ColorCode = "#e74c3c", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Category { Id = 3, Name = "Family", ColorCode = "#2ecc71", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Category { Id = 4, Name = "Social", ColorCode = "#f39c12", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Category { Id = 5, Name = "Health", ColorCode = "#9b59b6", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
