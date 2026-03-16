# Event Calendar API

A clean, production-ready ASP.NET Core 8 Web API for managing events, tickets, payments, venues, and user authentication using JWT.

---

## 🏗️ Architecture

```
EventCalendarAPI/
├── Controllers/         # API layer — handles HTTP requests & responses
│   ├── BaseController.cs
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── EventsController.cs
│   └── OtherControllers.cs   # Categories, Venues, Tickets, Payments
├── Services/            # Business logic layer
│   ├── AuthAndUserService.cs
│   ├── EventCategoryVenueService.cs
│   ├── TicketAndPaymentService.cs
│   └── PasswordAndTokenService.cs
├── Repositories/        # Data access layer (Repository pattern)
│   ├── Repository.cs    # Generic base repository
│   └── Repositories.cs  # All specific repositories
├── Interfaces/          # Contracts / abstractions
│   ├── IRepositories.cs
│   └── IServices.cs
├── Models/              # EF Core entity models
│   ├── User.cs
│   ├── Event.cs
│   ├── Category.cs
│   ├── Venue.cs
│   ├── Ticket.cs
│   └── Payment.cs
├── DTOs/
│   ├── Request/         # Input DTOs with validation
│   └── Response/        # Output DTOs
├── Data/
│   └── ApplicationDbContext.cs
├── Exceptions/
│   └── AppExceptions.cs
├── Helpers/
│   └── GlobalExceptionMiddleware.cs
└── Program.cs           # Startup + DI registration
```

---

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- MS SQL Server (local or Azure)

### Setup

1. **Clone / copy the project**

2. **Update `appsettings.json`** — set your SQL Server connection string and JWT secret:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.;Database=EventCalendarDB;Trusted_Connection=True;TrustServerCertificate=True;"
     },
     "Jwt": {
       "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
       "Issuer": "EventCalendarAPI",
       "Audience": "EventCalendarApp",
       "ExpiryHours": "24"
     }
   }
   ```

3. **Run EF Core migrations:**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Run the API:**
   ```bash
   dotnet run
   ```

5. **Open Swagger UI:** `https://localhost:5001/swagger`

---

## 🔐 Authentication

All protected endpoints require a `Bearer` JWT token in the `Authorization` header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Roles:**
- `User` — Default role, can manage own events/tickets
- `Admin` — Full access to all resources

---

## 📡 API Endpoints

### Auth
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register new account | ❌ |
| POST | `/api/auth/login` | Login, get JWT token | ❌ |

### Users
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/users` | Get all users | Admin |
| GET | `/api/users/{id}` | Get user by ID | ✅ |
| GET | `/api/users/me` | Get current user profile | ✅ |
| PUT | `/api/users/{id}` | Update profile | ✅ (own) |
| PUT | `/api/users/me/change-password` | Change password | ✅ |
| DELETE | `/api/users/{id}` | Deactivate account | ✅ (own) |

### Events
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/events` | Get all events (with filters) | ❌ |
| GET | `/api/events/{id}` | Get event by ID | ❌ |
| GET | `/api/events/my-events` | Get current user's events | ✅ |
| POST | `/api/events` | Create event | ✅ |
| PUT | `/api/events/{id}` | Update event | ✅ (owner) |
| DELETE | `/api/events/{id}` | Delete event | ✅ (owner) |

**Event Search Query Parameters:**
- `keyword` — search in title, description, location
- `categoryId` — filter by category
- `startDate` / `endDate` — date range filter
- `privacy` — Public / Private / InviteOnly
- `page` / `pageSize` — pagination

### Categories
| Method | Endpoint | Auth |
|--------|----------|------|
| GET | `/api/categories` | ❌ |
| GET | `/api/categories/{id}` | ❌ |
| POST | `/api/categories` | Admin |
| PUT | `/api/categories/{id}` | Admin |
| DELETE | `/api/categories/{id}` | Admin |

### Venues
| Method | Endpoint | Auth |
|--------|----------|------|
| GET | `/api/venues` | ❌ |
| GET | `/api/venues/{id}` | ❌ |
| POST | `/api/venues` | ✅ |
| PUT | `/api/venues/{id}` | ✅ |
| DELETE | `/api/venues/{id}` | Admin |

### Tickets
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/tickets` | All tickets | Admin |
| GET | `/api/tickets/{id}` | Ticket by ID | ✅ (own) |
| GET | `/api/tickets/my-tickets` | Current user's tickets | ✅ |
| GET | `/api/tickets/by-event/{eventId}` | Tickets for event | ✅ (organizer) |
| POST | `/api/tickets` | Book a ticket | ✅ |
| PUT | `/api/tickets/{id}` | Update ticket | ✅ (own) |
| DELETE | `/api/tickets/{id}` | Cancel ticket | ✅ (own) |

### Payments
| Method | Endpoint | Auth |
|--------|----------|------|
| GET | `/api/payments` | All payments | Admin |
| GET | `/api/payments/{id}` | Payment by ID | ✅ |
| GET | `/api/payments/by-ticket/{ticketId}` | Payments for ticket | ✅ |
| POST | `/api/payments` | Process payment | ✅ |
| PUT | `/api/payments/{id}` | Update payment | Admin |
| DELETE | `/api/payments/{id}` | Delete payment | Admin |

---

## 📦 Response Format

All responses follow a consistent envelope:

```json
{
  "success": true,
  "message": "Success",
  "data": { ... },
  "errors": null
}
```

Paginated responses:
```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 100,
    "page": 1,
    "pageSize": 20,
    "totalPages": 5
  }
}
```

---

## 🗄️ Data Models

```
User ──< Event ──< Ticket ──< Payment
User ──< Ticket
Category ──< Event
Venue ──< Event
```

---

## 🔧 Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Database | MS SQL Server |
| Auth | JWT Bearer Tokens |
| Password Hashing | PBKDF2-SHA512 (350,000 iterations) |
| API Docs | Swagger / OpenAPI |
| Architecture | Repository + Service pattern |
