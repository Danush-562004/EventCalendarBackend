using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.Exceptions;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;
using EventCalendarAPI.Services;
using Moq;

namespace EventCalendarAPI.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepoMock = new();
        private readonly Mock<ITicketRepository> _ticketRepoMock = new();
        private readonly PaymentService _sut;

        public PaymentServiceTests()
        {
            _sut = new PaymentService(_paymentRepoMock.Object, _ticketRepoMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ThrowsEntityNotFoundException()
        {
            _paymentRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Payment?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.GetByIdAsync(99));
        }

        [Fact]
        public async Task GetAllAsync_ReturnsPaged()
        {
            var payments = new List<Payment> { new Payment { Id = 1, Amount = 100, Currency = "USD", Status = PaymentStatus.Completed, Method = PaymentMethod.CreditCard } };
            _paymentRepoMock.Setup(r => r.GetPagedAsync(1, 10)).ReturnsAsync(new PagedResult<Payment> { Items = payments, TotalCount = 1 });

            var result = await _sut.GetAllAsync(1, 10);

            Assert.Single(result.Items);
        }

        [Fact]
        public async Task CreateAsync_WhenTicketCancelled_ThrowsValidationException()
        {
            var ticket = new Ticket { Id = 1, Status = TicketStatus.Cancelled };
            _ticketRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

            await Assert.ThrowsAsync<ValidationException>(() =>
                _sut.CreateAsync(new CreatePaymentRequestDto { TicketId = 1, Amount = 100, Method = PaymentMethod.CreditCard }));
        }

        [Fact]
        public async Task CreateAsync_WithValidTicket_ConfirmsTicketAndReturnsPayment()
        {
            var ticket = new Ticket { Id = 1, Status = TicketStatus.Reserved };
            _ticketRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);
            _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);
            _ticketRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Ticket>())).ReturnsAsync((Ticket t) => t);

            var result = await _sut.CreateAsync(new CreatePaymentRequestDto { TicketId = 1, Amount = 100, Method = PaymentMethod.CreditCard });

            Assert.Equal(TicketStatus.Confirmed, ticket.Status);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByTicketIdAsync_WhenTicketNotFound_ThrowsEntityNotFoundException()
        {
            _ticketRepoMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.GetByTicketIdAsync(99));
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ThrowsEntityNotFoundException()
        {
            _paymentRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Payment?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _sut.UpdateAsync(99, new UpdatePaymentRequestDto()));
        }
    }
}
