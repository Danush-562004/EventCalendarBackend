using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Exceptions;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;
namespace EventCalendarAPI.Services
{
    // ─── Ticket Service ──────────────────────────────────────────
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IAuditLogService _auditLog;

        public TicketService(ITicketRepository ticketRepository, IEventRepository eventRepository, IAuditLogService auditLog)
        {
            _ticketRepository = ticketRepository;
            _eventRepository = eventRepository;
            _auditLog = auditLog;
        }

        public async Task<TicketResponseDto> GetByIdAsync(int id, int requestingUserId, string userRole)
        {
            var ticket = await _ticketRepository.GetByIdWithDetailsAsync(id)
                ?? throw new EntityNotFoundException("Ticket", id);

            if (userRole != "Admin" && ticket.UserId != requestingUserId)
                throw new UnauthorizedException("You can only view your own tickets.");

            return MapToResponse(ticket);
        }

        public async Task<PagedResponseDto<TicketResponseDto>> GetAllAsync(int page, int pageSize, string? status = null)
        {
            var result = await _ticketRepository.GetPagedAsync(page, pageSize, status);
            return new PagedResponseDto<TicketResponseDto>
            {
                Items = result.Items.Select(MapToResponse).ToList(),
                TotalCount = result.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<TicketResponseDto>> GetByEventIdAsync(int eventId, int requestingUserId, string userRole)
        {
            var ev = await _eventRepository.GetByIdAsync(eventId)
                ?? throw new EntityNotFoundException("Event", eventId);

            if (userRole != "Admin" && ev.UserId != requestingUserId)
                throw new UnauthorizedException("You can only view tickets for your own events.");

            var tickets = await _ticketRepository.GetByEventIdAsync(eventId);
            return tickets.Select(MapToResponse);
        }

        public async Task<IEnumerable<TicketResponseDto>> GetByCurrentUserAsync(int userId)
        {
            var tickets = await _ticketRepository.GetByUserIdAsync(userId);
            return tickets.Select(MapToResponse);
        }

        public async Task<TicketResponseDto> CreateAsync(CreateTicketRequestDto request, int userId)
        {
            var ev = await _eventRepository.GetByIdWithDetailsAsync(request.EventId)
                ?? throw new EntityNotFoundException("Event", request.EventId);

            if (!ev.IsActive)
                throw new ValidationException("Cannot book tickets for an inactive event.");

            if (ev.EndDateTime <= DateTime.UtcNow)
                throw new ValidationException("Cannot book tickets for a past event.");

            // Per-user ticket limit: max 10 tickets per user per event
            var userEventTickets = await _ticketRepository.GetByUserIdAsync(userId);
            int userEventTotal = userEventTickets
                .Where(t => t.EventId == request.EventId && t.Status != TicketStatus.Cancelled)
                .Sum(t => t.Quantity);
            Console.WriteLine($"danush {userEventTotal}");
            if (userEventTotal + request.Quantity > 10)

                throw new ValidationException($"Ticket limit reached. You can book at most 10 tickets per event. You already have {userEventTotal} for this event.");

            // Capacity check
            int bookedCount = ev.Tickets?.Where(t => t.Status != TicketStatus.Cancelled)
                                         .Sum(t => t.Quantity) ?? 0; 
            //count returns the matching record count, sum returns sum of ticket quantity in all records
            int available = ev.MaxAttendees - bookedCount;
            if (ev.MaxAttendees > 0 && available < request.Quantity)
                throw new ValidationException($"Only {available} seats available.");

            var ticket = new Ticket
            {
                TicketNumber = GenerateTicketNumber(),
                EventId = request.EventId,
                UserId = userId,
                Type = request.Type,
                Price = request.Type == TicketType.Free ? 0 : ev.Price,  // ✅ use event price
                Quantity = request.Quantity,
                SeatNumber = request.SeatNumber,
                Status = TicketStatus.Reserved
            };

            await _ticketRepository.AddAsync(ticket);
            var created = await _ticketRepository.GetByIdWithDetailsAsync(ticket.Id)!;
            await _auditLog.LogAsync("Create", "Ticket", ticket.Id.ToString(), userId, null,
                newValues: $"{{\"eventId\":{request.EventId},\"quantity\":{request.Quantity}}}");
            return MapToResponse(created!);
        }

        private static decimal GetPriceByType(TicketType type) => type switch
        {
            TicketType.Free => 0,
            TicketType.Paid => 1000,
            TicketType.VIP => 1500,
            _ => 0
        };

        public async Task<TicketResponseDto> UpdateAsync(int id, UpdateTicketRequestDto request, int requestingUserId, string userRole)
        {
            var ticket = await _ticketRepository.GetByIdWithDetailsAsync(id)
                ?? throw new EntityNotFoundException("Ticket", id);

            if (userRole != "Admin" && ticket.UserId != requestingUserId)
                throw new UnauthorizedException("You can only update your own tickets.");

            if (request.Status.HasValue) ticket.Status = request.Status.Value;
            if (request.SeatNumber != null) ticket.SeatNumber = request.SeatNumber;
            if (request.CheckedIn.HasValue)
            {
                ticket.CheckedIn = request.CheckedIn.Value;
                if (request.CheckedIn.Value && ticket.CheckInTime == null)
                    ticket.CheckInTime = DateTime.UtcNow;
            }
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket);
            var updated = await _ticketRepository.GetByIdWithDetailsAsync(ticket.Id)!;
            return MapToResponse(updated!);
        }

        public async Task DeleteAsync(int id, int requestingUserId, string userRole)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Ticket", id);

            if (userRole != "Admin" && ticket.UserId != requestingUserId)
                throw new UnauthorizedException("You can only cancel your own tickets.");

            ticket.Status = TicketStatus.Cancelled;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _ticketRepository.UpdateAsync(ticket);
            await _auditLog.LogAsync("Cancel", "Ticket", id.ToString(), requestingUserId);
        }

        private static string GenerateTicketNumber() =>
            $"TKT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        public static TicketResponseDto MapToResponse(Ticket t) => new()
        {
            Id = t.Id,
            TicketNumber = t.TicketNumber,
            Type = t.Type.ToString(),
            Status = t.Status.ToString(),
            Price = t.Price,
            Quantity = t.Quantity,
            SeatNumber = t.SeatNumber,
            CheckedIn = t.CheckedIn,
            CheckInTime = t.CheckInTime,
            CreatedAt = t.CreatedAt,
            PaymentDeadline = t.CreatedAt.AddMinutes(5),
            EventId = t.EventId,
            EventTitle = t.Event?.Title ?? string.Empty,
            EventEndDateTime = t.Event?.EndDateTime ?? DateTime.MinValue,
            UserId = t.UserId,
            UserFullName = t.User != null ? $"{t.User.FirstName} {t.User.LastName}" : string.Empty,
            UserEmail = t.User?.Email ?? string.Empty,
            Payments = t.Payments?.Select(p => new PaymentResponseDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status.ToString(),
                Method = p.Method.ToString(),
                TransactionId = p.TransactionId,
                Notes = p.Notes,
                PaymentDate = p.PaymentDate,
                CreatedAt = p.CreatedAt,
                TicketId = p.TicketId
            }).ToList() ?? new List<PaymentResponseDto>()
        };
    }

    // ─── Payment Service ─────────────────────────────────────────
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IAuditLogService _auditLog;
        private readonly IEmailService _emailService;

        public PaymentService(IPaymentRepository paymentRepository, ITicketRepository ticketRepository,
            IAuditLogService auditLog, IEmailService emailService)
        {
            _paymentRepository = paymentRepository;
            _ticketRepository = ticketRepository;
            _auditLog = auditLog;
            _emailService = emailService;
        }

        public async Task<PaymentResponseDto> GetByIdAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Payment", id);
            return MapToResponse(payment);
        }

        public async Task<PagedResponseDto<PaymentResponseDto>> GetAllAsync(int page, int pageSize, string? status = null)
        {
            var result = await _paymentRepository.GetPagedAsync(page, pageSize, status);
            return new PagedResponseDto<PaymentResponseDto>
            {
                Items = result.Items.Select(MapToResponse).ToList(),
                TotalCount = result.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetByTicketIdAsync(int ticketId)
        {
            if (!await _ticketRepository.ExistsAsync(ticketId))
                throw new EntityNotFoundException("Ticket", ticketId);

            var payments = await _paymentRepository.GetByTicketIdAsync(ticketId);
            return payments.Select(MapToResponse);
        }

        public async Task<PaymentResponseDto> CreateAsync(CreatePaymentRequestDto request)
        {
            var ticket = await _ticketRepository.GetByIdWithDetailsAsync(request.TicketId)
                ?? throw new EntityNotFoundException("Ticket", request.TicketId);

            if (ticket.Status == TicketStatus.Cancelled)
                throw new ValidationException("Cannot process payment for a cancelled ticket.");

            if (ticket.Event != null && ticket.Event.EndDateTime <= DateTime.UtcNow)
                throw new ValidationException("Cannot process payment — the event has already ended.");

            // 5-minute payment window
            if (DateTime.UtcNow > ticket.CreatedAt.AddMinutes(5))
                throw new ValidationException("Payment window expired. Please book a new ticket.");

            var payment = new Payment
            {
                TicketId = request.TicketId,
                Amount = request.Amount,
                Currency = request.Currency,
                Method = Enum.Parse<PaymentMethod>(request.Method, ignoreCase: true),
                TransactionId = request.TransactionId,
                Notes = request.Notes,
                Status = PaymentStatus.Completed,
                PaymentDate = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);

            ticket.Status = TicketStatus.Confirmed;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _ticketRepository.UpdateAsync(ticket);
            await _auditLog.LogAsync("Create", "Payment", payment.Id.ToString(), null, null,
                newValues: $"{{\"ticketId\":{request.TicketId},\"amount\":{request.Amount},\"method\":\"{request.Method}\"}}");

            // Send ticket confirmation email
            if (ticket.User != null)
            {
                _ = _emailService.SendAsync(
                    ticket.User.Email,
                    ticket.User.FirstName,
                    $"🎫 Booking Confirmed: {ticket.Event?.Title}",
                    BuildTicketEmail(ticket, payment)
                );
            }

            return MapToResponse(payment);
        }

        public async Task<PaymentResponseDto> UpdateAsync(int id, UpdatePaymentRequestDto request)
        {
            var payment = await _paymentRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Payment", id);

            if (request.Status.HasValue) payment.Status = request.Status.Value;
            if (request.TransactionId != null) payment.TransactionId = request.TransactionId;
            if (request.PaymentGatewayReference != null) payment.PaymentGatewayReference = request.PaymentGatewayReference;
            if (request.Notes != null) payment.Notes = request.Notes;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);
            return MapToResponse(payment);
        }

        public async Task DeleteAsync(int id)
        {
            await _paymentRepository.DeleteAsync(id);
        }

        private static string BuildTicketEmail(Ticket ticket, Payment payment)
        {
            var eventTitle   = ticket.Event?.Title ?? "Event";
            var eventDate    = ticket.Event?.StartDateTime.ToString("dddd, MMMM d yyyy 'at' h:mm tt") + " UTC" ?? "";
            var venue        = ticket.Event?.Venue != null
                ? $"{ticket.Event.Venue.Name}, {ticket.Event.Venue.City}"
                : ticket.Event?.Location ?? "—";
            var seatInfo     = string.IsNullOrWhiteSpace(ticket.SeatNumber) ? "—" : ticket.SeatNumber;

            return $"""
                <!DOCTYPE html>
                <html>
                <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;'>
                  <table width='100%' cellpadding='0' cellspacing='0'>
                    <tr><td align='center' style='padding:40px 0;'>
                      <table width='560' cellpadding='0' cellspacing='0'
                             style='background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.08);'>
                        <tr>
                          <td style='background:#4f46e5;padding:32px 40px;text-align:center;'>
                            <h1 style='margin:0;color:#ffffff;font-size:24px;'>🎫 Booking Confirmed!</h1>
                          </td>
                        </tr>
                        <tr>
                          <td style='padding:36px 40px;'>
                            <p style='font-size:16px;color:#333;'>Hi <strong>{ticket.User!.FirstName}</strong>,</p>
                            <p style='font-size:15px;color:#555;'>Your payment was successful. Here are your ticket details:</p>

                            <div style='background:#f0f0ff;border-left:4px solid #4f46e5;border-radius:8px;padding:20px 24px;margin:20px 0;'>
                              <h2 style='margin:0 0 12px;color:#4f46e5;font-size:20px;'>{eventTitle}</h2>
                              <table cellpadding='0' cellspacing='0' width='100%'>
                                <tr><td style='font-size:13px;color:#555;padding:4px 0;width:140px;'>📅 Date</td><td style='font-size:13px;color:#333;font-weight:600;'>{eventDate}</td></tr>
                                <tr><td style='font-size:13px;color:#555;padding:4px 0;'>📍 Venue</td><td style='font-size:13px;color:#333;font-weight:600;'>{venue}</td></tr>
                                <tr><td style='font-size:13px;color:#555;padding:4px 0;'>🎫 Ticket #</td><td style='font-size:13px;color:#333;font-weight:600;font-family:monospace;'>{ticket.TicketNumber}</td></tr>
                                <tr><td style='font-size:13px;color:#555;padding:4px 0;'>🪑 Seat</td><td style='font-size:13px;color:#333;font-weight:600;'>{seatInfo}</td></tr>
                                <tr><td style='font-size:13px;color:#555;padding:4px 0;'>🎟 Quantity</td><td style='font-size:13px;color:#333;font-weight:600;'>{ticket.Quantity}</td></tr>
                                <tr><td style='font-size:13px;color:#555;padding:4px 0;'>🏷 Type</td><td style='font-size:13px;color:#333;font-weight:600;'>{ticket.Type}</td></tr>
                              </table>
                            </div>

                            <div style='background:#f0fdf4;border-left:4px solid #22c55e;border-radius:8px;padding:16px 24px;margin:20px 0;'>
                              <table cellpadding='0' cellspacing='0' width='100%'>
                                <tr><td style='font-size:13px;color:#555;padding:4px 0;width:140px;'>💳 Method</td><td style='font-size:13px;color:#333;font-weight:600;'>{payment.Method}</td></tr>
                                <tr><td style='font-size:13px;color:#555;padding:4px 0;'>💰 Amount Paid</td><td style='font-size:15px;color:#15803d;font-weight:700;'>₹{payment.Amount:N2} {payment.Currency}</td></tr>
                                <tr><td style='font-size:13px;color:#555;padding:4px 0;'>🔖 Transaction ID</td><td style='font-size:12px;color:#333;font-family:monospace;'>{payment.TransactionId ?? "—"}</td></tr>
                              </table>
                            </div>

                            <p style='font-size:13px;color:#999;margin-top:32px;'>
                              Please keep this email as your booking confirmation. See you at the event!
                            </p>
                          </td>
                        </tr>
                        <tr>
                          <td style='background:#f9f9f9;padding:16px 40px;text-align:center;'>
                            <p style='margin:0;font-size:12px;color:#aaa;'>© Event Calendar</p>
                          </td>
                        </tr>
                      </table>
                    </td></tr>
                  </table>
                </body>
                </html>
                """;
        }

        public static PaymentResponseDto MapToResponse(Payment p) => new()        {
            Id = p.Id,
            Amount = p.Amount,
            //Quantity=p.Ticket?.Quantity?? 0,
            //TotalPrice =p.Amount * (p.Ticket?.Quantity ?? 0),
            Currency = p.Currency,
            Status = p.Status.ToString(),
            Method = p.Method.ToString(),
            TransactionId = p.TransactionId,
            Notes = p.Notes,
            PaymentDate = p.PaymentDate,
            CreatedAt = p.CreatedAt,
            TicketId = p.TicketId
        };
    }
}
