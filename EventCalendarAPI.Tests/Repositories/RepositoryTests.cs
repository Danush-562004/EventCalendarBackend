using EventCalendarAPI.Data;
using EventCalendarAPI.Models;
using EventCalendarAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventCalendarAPI.Tests.Repositories
{
    public class RepositoryTests
    {
        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        // ─── UserRepository ──────────────────────────────────────

        [Fact]
        public async Task UserRepository_AddAndGetById_Works()
        {
            using var ctx = CreateContext(nameof(UserRepository_AddAndGetById_Works));
            var repo = new UserRepository(ctx);
            var user = new User { Username = "john", Email = "john@test.com", FirstName = "John", LastName = "Doe", PasswordHash = "h", PasswordSalt = new byte[64] };

            await repo.AddAsync(user);
            var found = await repo.GetByIdAsync(user.Id);

            Assert.NotNull(found);
            Assert.Equal("john", found!.Username);
        }

        [Fact]
        public async Task UserRepository_UsernameExistsAsync_ReturnsTrue()
        {
            using var ctx = CreateContext(nameof(UserRepository_UsernameExistsAsync_ReturnsTrue));
            var repo = new UserRepository(ctx);
            await repo.AddAsync(new User { Username = "john", Email = "j@j.com", FirstName = "J", LastName = "D", PasswordHash = "h", PasswordSalt = new byte[64] });

            var exists = await repo.UsernameExistsAsync("john");

            Assert.True(exists);
        }

        [Fact]
        public async Task UserRepository_EmailExistsAsync_ReturnsFalse_WhenNotExists()
        {
            using var ctx = CreateContext(nameof(UserRepository_EmailExistsAsync_ReturnsFalse_WhenNotExists));
            var repo = new UserRepository(ctx);

            var exists = await repo.EmailExistsAsync("nobody@test.com");

            Assert.False(exists);
        }

        [Fact]
        public async Task UserRepository_GetByUsernameOrEmail_ReturnsUser()
        {
            using var ctx = CreateContext(nameof(UserRepository_GetByUsernameOrEmail_ReturnsUser));
            var repo = new UserRepository(ctx);
            await repo.AddAsync(new User { Username = "john", Email = "john@test.com", FirstName = "J", LastName = "D", PasswordHash = "h", PasswordSalt = new byte[64] });

            var user = await repo.GetByUsernameOrEmailAsync("john@test.com");

            Assert.NotNull(user);
        }

        [Fact]
        public async Task UserRepository_GetPagedAsync_ReturnsPaged()
        {
            using var ctx = CreateContext(nameof(UserRepository_GetPagedAsync_ReturnsPaged));
            var repo = new UserRepository(ctx);
            for (int i = 1; i <= 5; i++)
                await repo.AddAsync(new User { Username = $"user{i}", Email = $"u{i}@test.com", FirstName = "F", LastName = "L", PasswordHash = "h", PasswordSalt = new byte[64] });

            var result = await repo.GetPagedAsync(1, 3);

            Assert.Equal(3, result.Items.Count);
            Assert.Equal(5, result.TotalCount);
        }

        // ─── CategoryRepository ───────────────────────────────────

        [Fact]
        public async Task CategoryRepository_NameExistsAsync_ReturnsTrue()
        {
            using var ctx = CreateContext(nameof(CategoryRepository_NameExistsAsync_ReturnsTrue));
            var repo = new CategoryRepository(ctx);
            await repo.AddAsync(new Category { Name = "Work", ColorCode = "#e74c3c" });

            var exists = await repo.NameExistsAsync("work");

            Assert.True(exists);
        }

        [Fact]
        public async Task CategoryRepository_GetPagedAsync_ReturnsPaged()
        {
            using var ctx = CreateContext(nameof(CategoryRepository_GetPagedAsync_ReturnsPaged));
            var repo = new CategoryRepository(ctx);
            await repo.AddAsync(new Category { Name = "Work", ColorCode = "#e74c3c", IsActive = true });
            await repo.AddAsync(new Category { Name = "Personal", ColorCode = "#3498db", IsActive = true });

            var result = await repo.GetPagedAsync(1, 10);

            Assert.Equal(2, result.TotalCount);
        }

        // ─── VenueRepository ──────────────────────────────────────

        [Fact]
        public async Task VenueRepository_GetActiveVenuesAsync_ReturnsOnlyActive()
        {
            using var ctx = CreateContext(nameof(VenueRepository_GetActiveVenuesAsync_ReturnsOnlyActive));
            var repo = new VenueRepository(ctx);
            await repo.AddAsync(new Venue { Name = "Active", Address = "A", City = "C", State = "S", Country = "US", Capacity = 100, IsActive = true });
            await repo.AddAsync(new Venue { Name = "Inactive", Address = "B", City = "C", State = "S", Country = "US", Capacity = 100, IsActive = false });

            var venues = await repo.GetActiveVenuesAsync();

            Assert.Single(venues);
        }

        // ─── ReminderRepository ───────────────────────────────────

        [Fact]
        public async Task ReminderRepository_AddAndGetByUserId_Works()
        {
            using var ctx = CreateContext(nameof(ReminderRepository_AddAndGetByUserId_Works));

            // Seed user and event
            var user = new User { Username = "john", Email = "j@j.com", FirstName = "J", LastName = "D", PasswordHash = "h", PasswordSalt = new byte[64] };
            ctx.Users.Add(user);
            var category = new Category { Name = "Work", ColorCode = "#e74c3c" };
            ctx.Categories.Add(category);
            await ctx.SaveChangesAsync();

            var ev = new Event { Title = "Event", StartDateTime = DateTime.UtcNow.AddDays(1), EndDateTime = DateTime.UtcNow.AddDays(2), UserId = user.Id, CategoryId = category.Id, Privacy = EventPrivacy.Public, Recurrence = RecurrencePattern.None };
            ctx.Events.Add(ev);
            await ctx.SaveChangesAsync();

            var repo = new ReminderRepository(ctx);
            await repo.AddAsync(new Reminder { Title = "Reminder", ReminderDateTime = DateTime.UtcNow.AddHours(1), EventId = ev.Id, UserId = user.Id, IsActive = true });

            var reminders = await repo.GetByUserIdAsync(user.Id);

            Assert.Single(reminders);
        }
    }
}
