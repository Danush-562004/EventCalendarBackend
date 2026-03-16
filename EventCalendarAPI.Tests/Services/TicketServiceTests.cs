using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.Exceptions;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;
using EventCalendarAPI.Services;
using Moq;

namespace EventCalendarAPI.Tests.Services
{
    public class TicketServiceTests
    {
        private readonly Mock<ITicketRepository> _ticketRepoMock = new();
        private readonly Mock<IEventRepository> _eventRepoMock = new();
        private readonly TicketService _sut;

        public TicketServiceTests()
        {
            _sut = new TicketService(_ticketRepoMock.Object, _eventRepoMock.Object);
        }

        private static Ticket BuildTicket(int id = 1, int userId = 1) => new()
        {
            Id = id,
            TicketNumber = "TKT-001",
            UserId = userId,
            EventId = 1,
            Type = TicketType.Free,
            Status = TicketStatus.Reserved,
            Event = new Event { Id = 1, Title = "Test Event", StartDateTime = DateTime.UtcNow.AddDays(1), EndDateTime = DateTime.UtcNow.AddDays(2), UserId = 1, IsActive = true, Privacy = EventPrivacy.Public, Recurrence = RecurrencePattern.None, Category = new Category { Id = 1, Name = "Work", ColorCode = "#e74c3c" } },
            User = new User { Id = userId, FirstName = "John", LastName = "Doe" },
            Payments = new List<Payment>()
        };

        [Fact]
        public async Task GetByIdAsync_WhenNotOwnerAndNotAdmin_ThrowsUnauthorizedException()
        {
            _ticketRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(BuildTicket(userId: 2));

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _sut.GetByIdAsync(1, requestingUserId: 1, userRole: "User"));
        }

        [Fact]
        public async Task GetByIdAsync_WhenAdmin_ReturnsDto()
        {
            _ticketRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(BuildTicket(userId: 2));

            var result = await _sut.GetByIdAsync(1, requestingUserId: 1, userRole: "Admin");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsPaged()
        {
            var tickets = new List<Ticket> { BuildTicket() };
            _ticketRepoMock.Setup(r => r.GetPagedAsync(1, 10)).ReturnsAsync(new PagedResult<Ticket> { Items = tickets, TotalCount = 1 });

            var result = await _sut.GetAllAsync(1, 10);

            Assert.Single(result.Items);
        }

        [Fact]
        public async Task CreateAsync_WhenEventInactive_ThrowsValidationException()
        {
            var ev = new Event { Id = 1, IsActive = false, Tickets = new List<Ticket>(), Category = new Category { Id = 1, Name = "Work", ColorCode = "#e74c3c" }, User = new User { FirstName = "J", LastName = "D" }, Privacy = EventPrivacy.Public, Recurrence = RecurrencePattern.None };
            _eventRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(ev);

            await Assert.ThrowsAsync<ValidationException>(() =>
                _sut.CreateAsync(new CreateTicketRequestDto { EventId = 1 }, userId: 1));
        }

        [Fact]
        public async Task CreateAsync_WhenCapacityFull_ThrowsValidationException()
        {
            var tickets = Enumerable.Range(1, 5).Select(i => new Ticket()).ToList();
            var ev = new Event { Id = 1, IsActive = true, MaxAttendees = 5, Tickets = tickets, Category = new Category { Id = 1, Name = "Work", ColorCode = "#e74c3c" }, User = new User { FirstName = "J", LastName = "D" }, Privacy = EventPrivacy.Public, Recurrence = RecurrencePattern.None };
            _eventRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(ev);

            await Assert.ThrowsAsync<ValidationException>(() =>
                _sut.CreateAsync(new CreateTicketRequestDto { EventId = 1 }, userId: 1));
        }

        [Fact]
        public async Task DeleteAsync_WhenNotOwnerAndNotAdmin_ThrowsUnauthorizedException()
        {
            _ticketRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildTicket(userId: 2));

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _sut.DeleteAsync(1, requestingUserId: 1, userRole: "User"));
        }

        [Fact]
        public async Task DeleteAsync_WhenOwner_CancelsTicket()
        {
            var ticket = BuildTicket(userId: 1);
            _ticketRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);
            _ticketRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Ticket>())).ReturnsAsync((Ticket t) => t);

            await _sut.DeleteAsync(1, requestingUserId: 1, userRole: "User");

            Assert.Equal(TicketStatus.Cancelled, ticket.Status);
        }
    }
}
