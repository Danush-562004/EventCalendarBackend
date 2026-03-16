using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.Exceptions;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;
using EventCalendarAPI.Services;
using Moq;

namespace EventCalendarAPI.Tests.Services
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _eventRepoMock = new();
        private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
        private readonly EventService _sut;

        public EventServiceTests()
        {
            _sut = new EventService(_eventRepoMock.Object, _categoryRepoMock.Object);
        }

        private static Event BuildEvent(int id = 1, int userId = 1) => new()
        {
            Id = id,
            Title = "Test Event",
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(2),
            UserId = userId,
            IsActive = true,
            Privacy = EventPrivacy.Public,
            Recurrence = RecurrencePattern.None,
            Category = new Category { Id = 1, Name = "Work", ColorCode = "#e74c3c" },
            User = new User { Id = userId, FirstName = "John", LastName = "Doe" },
            Tickets = new List<Ticket>()
        };

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ThrowsEntityNotFoundException()
        {
            _eventRepoMock.Setup(r => r.GetByIdWithDetailsAsync(99)).ReturnsAsync((Event?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.GetByIdAsync(99));
        }

        [Fact]
        public async Task GetByIdAsync_WhenFound_ReturnsDto()
        {
            _eventRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(BuildEvent());

            var result = await _sut.GetByIdAsync(1);

            Assert.Equal("Test Event", result.Title);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsPaged()
        {
            var events = new List<Event> { BuildEvent() };
            _eventRepoMock.Setup(r => r.GetAllPagedAsync(1, 10)).ReturnsAsync(new PagedResult<Event> { Items = events, TotalCount = 1 });

            var result = await _sut.GetAllAsync(1, 10);

            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task CreateAsync_WhenStartAfterEnd_ThrowsValidationException()
        {
            var request = new CreateEventRequestDto
            {
                Title = "Bad Event",
                StartDateTime = DateTime.UtcNow.AddDays(2),
                EndDateTime = DateTime.UtcNow.AddDays(1),
                CategoryId = 1
            };

            await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(request, userId: 1));
        }

        [Fact]
        public async Task CreateAsync_WhenCategoryNotFound_ThrowsEntityNotFoundException()
        {
            var request = new CreateEventRequestDto
            {
                Title = "Event",
                StartDateTime = DateTime.UtcNow.AddDays(1),
                EndDateTime = DateTime.UtcNow.AddDays(2),
                CategoryId = 99
            };
            _categoryRepoMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.CreateAsync(request, userId: 1));
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsDto()
        {
            var request = new CreateEventRequestDto
            {
                Title = "New Event",
                StartDateTime = DateTime.UtcNow.AddDays(1),
                EndDateTime = DateTime.UtcNow.AddDays(2),
                CategoryId = 1
            };
            _categoryRepoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
            _eventRepoMock.Setup(r => r.AddAsync(It.IsAny<Event>())).ReturnsAsync((Event e) => e);
            _eventRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(BuildEvent());

            var result = await _sut.CreateAsync(request, userId: 1);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotOwner_ThrowsUnauthorizedException()
        {
            _eventRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(BuildEvent(userId: 2));

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _sut.UpdateAsync(1, new UpdateEventRequestDto(), userId: 1));
        }

        [Fact]
        public async Task DeleteAsync_WhenNotOwner_ThrowsUnauthorizedException()
        {
            _eventRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildEvent(userId: 2));

            await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.DeleteAsync(1, userId: 1));
        }

        [Fact]
        public async Task DeleteAsync_WhenOwner_SoftDeletes()
        {
            var ev = BuildEvent(userId: 1);
            _eventRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ev);
            _eventRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Event>())).ReturnsAsync((Event e) => e);

            await _sut.DeleteAsync(1, userId: 1);

            Assert.False(ev.IsActive);
        }
    }
}
