using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.Exceptions;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;
using EventCalendarAPI.Services;
using Moq;

namespace EventCalendarAPI.Tests.Services
{
    public class ReminderServiceTests
    {
        private readonly Mock<IReminderRepository> _reminderRepoMock = new();
        private readonly Mock<IEventRepository> _eventRepoMock = new();
        private readonly ReminderService _sut;

        public ReminderServiceTests()
        {
            _sut = new ReminderService(_reminderRepoMock.Object, _eventRepoMock.Object);
        }

        private static Reminder BuildReminder(int id = 1, int userId = 1) => new()
        {
            Id = id,
            Title = "Test Reminder",
            ReminderDateTime = DateTime.UtcNow.AddHours(1),
            Type = ReminderType.Email,
            EventId = 1,
            UserId = userId,
            IsActive = true,
            Event = new Event { Id = 1, Title = "Test Event", StartDateTime = DateTime.UtcNow.AddDays(1), EndDateTime = DateTime.UtcNow.AddDays(2), UserId = userId, IsActive = true, Privacy = EventPrivacy.Public, Recurrence = RecurrencePattern.None, Category = new Category { Id = 1, Name = "Work", ColorCode = "#e74c3c" } }
        };

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ThrowsEntityNotFoundException()
        {
            _reminderRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Reminder?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.GetByIdAsync(99, requestingUserId: 1));
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotOwner_ThrowsUnauthorizedException()
        {
            _reminderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildReminder(userId: 2));

            await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.GetByIdAsync(1, requestingUserId: 1));
        }

        [Fact]
        public async Task GetByIdAsync_WhenOwner_ReturnsDto()
        {
            _reminderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildReminder(userId: 1));

            var result = await _sut.GetByIdAsync(1, requestingUserId: 1);

            Assert.Equal("Test Reminder", result.Title);
        }

        [Fact]
        public async Task GetByCurrentUserAsync_ReturnsPaged()
        {
            var reminders = new List<Reminder> { BuildReminder() };
            _reminderRepoMock.Setup(r => r.GetPagedByUserAsync(1, 1, 10)).ReturnsAsync(new PagedResult<Reminder> { Items = reminders, TotalCount = 1 });

            var result = await _sut.GetByCurrentUserAsync(1, 1, 10);

            Assert.Single(result.Items);
        }

        [Fact]
        public async Task CreateAsync_WhenEventNotFound_ThrowsEntityNotFoundException()
        {
            _eventRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Event?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _sut.CreateAsync(new CreateReminderRequestDto { EventId = 99, Title = "Reminder", ReminderDateTime = DateTime.UtcNow.AddHours(1) }, userId: 1));
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsDto()
        {
            var ev = new Event { Id = 1, Title = "Event", StartDateTime = DateTime.UtcNow.AddDays(1), EndDateTime = DateTime.UtcNow.AddDays(2), UserId = 1, IsActive = true, Privacy = EventPrivacy.Public, Recurrence = RecurrencePattern.None, Category = new Category { Id = 1, Name = "Work", ColorCode = "#e74c3c" } };
            _eventRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ev);
            _reminderRepoMock.Setup(r => r.AddAsync(It.IsAny<Reminder>())).ReturnsAsync((Reminder r) => r);
            _reminderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(BuildReminder());

            var result = await _sut.CreateAsync(new CreateReminderRequestDto
            {
                EventId = 1,
                Title = "My Reminder",
                ReminderDateTime = DateTime.UtcNow.AddHours(1)
            }, userId: 1);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotOwner_ThrowsUnauthorizedException()
        {
            _reminderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildReminder(userId: 2));

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _sut.UpdateAsync(1, new UpdateReminderRequestDto(), requestingUserId: 1));
        }

        [Fact]
        public async Task DeleteAsync_WhenOwner_SoftDeletes()
        {
            var reminder = BuildReminder(userId: 1);
            _reminderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reminder);
            _reminderRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Reminder>())).ReturnsAsync((Reminder r) => r);

            await _sut.DeleteAsync(1, requestingUserId: 1);

            Assert.False(reminder.IsActive);
        }
    }
}
