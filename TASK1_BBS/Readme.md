I'm creating a bus booking system project.
Frontend : Angular
Backend : .NET Web API
Database : PostgreSQL
Authentication : JWT, Bcrypt, 

Roles:
* USER
* OPERATOR
* ADMIN


User:
* Register and Login
* Search Bus
* Book Seat
* Cancel Booking
* View Booking

Operator:
* Register and Login
* Add Bus
* Add Schedule
* Add Seat
* Update Seat
* Delete Seat
* View Booking
* Cancel Booking
* Add Refund

Admin:
* Register and Login
* Approve/Reject Operator
* Approve/Reject Booking
* Approve/Reject Refund
* View Booking
* Cancel Booking
* Add Refund

Workflow (User):
* Initially the bus source, destination and journey date page must be shown. 
* Then a list of buses must be shown according to the source, destination and journey date.
* Then the user can select a bus and book a seat. (but have to login before booking)
* Then the user can cancel a booking.
* Then the user can view the booking.
* Upon successful booking the user must receive an email notification and also in the ui, a booking confirmation must be shown.
* Similarly, if the booking is cancelled, the user must receive an email notification and also in the ui, a booking cancellation confirmation must be shown.

Operator:
* An operator can register their account by giving their details + office address at different cities (for now 1 office per city)
* If admin approves, the operator can login and add buses, delete bus..view bus bookings and set the ticket price per journey and can also see the revenue.

ADMIN:
*Admin should approve the operator accounts.
* admin can add new routes. (src and destination) Every route has source and destination point only no, along the route point.
* bus operator has a feature of adding the bus (associating a bus details to a bus route added by the admin). Admin has to approve the bus and then only it will be visible to the user.
* When the operator adds a bus to a route, the operator's office address will be shown as the pickup point and the destination address will be shown as the drop point.
* admin can view the overall bus booking and revenue generation details.

*For every bus, its number plate will be the unique id.
* It a user selects a bus, its layout must be visible. Implement bus layout with packages
* The bus operator also has an option to add new layout. 

Click any box to ask a deep-dive question about it. Here's the full breakdown:

---

## Frontend — Angular Modules

**Feature modules (lazy-loaded by role):**

`UserModule` — search page (source/dest/date), bus list results, seat picker with layout renderer, booking confirmation page, my-bookings page with cancel action.

`OperatorModule` — bus management (add/delete, associate to route), schedule management (date/time/price per journey), seat layout builder, booking viewer, refund submission, revenue dashboard.

`AdminModule` — operator approval list, bus approval list, route management (add src→dest), booking overview, refund approval, global revenue report.

**Shared / Core:**

`AuthModule` — login/register forms for all three roles, route guards (`canActivate` per role), JWT interceptor that attaches `Bearer` tokens to every outgoing request.

`NotificationModule` — toast service for booking confirmations and cancellations, integrates with backend polling or a WebSocket/SignalR channel.

`SeatLayoutModule` — renders a bus seat grid from a layout config (rows × columns, seat types); handles selection state; this can be a standalone Angular library or an npm package.

---

## Backend — .NET Web API Modules / Controllers

| Controller | Key endpoints |
|---|---|
| `AuthController` | `POST /register`, `POST /login` (all roles) |
| `UserController` | profile, booking history |
| `SearchController` | `GET /buses?src=&dest=&date=` |
| `BookingController` | create, cancel, view bookings |
| `OperatorController` | add/delete bus, add schedule, seat CRUD, revenue |
| `BusController` | associate bus to route, layout attach, status |
| `RouteController` | admin adds `{src, dest}` routes |
| `AdminController` | approve/reject operators, buses, bookings, refunds |
| `SeatLayoutController` | create/update layout templates |
| `RefundController` | operator/admin submit refund, admin approve |
| `NotificationController` | internal — triggers email via SendGrid/Mailgun |

**Cross-cutting concerns:** JWT middleware validates token on every request; a `RoleRequirement` attribute gates endpoints by `USER / OPERATOR / ADMIN`; a global exception filter returns consistent error envelopes; EF Core with Npgsql is the ORM.

---

## Database — PostgreSQL Tables

**Core tables:** `Users` (id, email, password_hash, role, is_approved), `Operators` (user_id FK, company_name, office_addresses[], status), `Routes` (id, source, destination), `Buses` (number_plate PK, operator_id FK, route_id FK, layout_id FK, status), `Schedules` (id, bus_id FK, journey_date, departure_time, price_per_seat), `SeatLayouts` (id, name, rows, cols, seat_config JSONB), `Seats` (id, schedule_id FK, seat_number, type, is_available), `Bookings` (id, user_id FK, seat_id FK, schedule_id FK, status, booked_at), `Refunds` (id, booking_id FK, amount, status, processed_by), `Approvals` (id, entity_type, entity_id, admin_id FK, status, note).

---

## Key Workflows

**User booking flow:** Search (no auth) → results list → click bus → seat layout renders (auth required) → select seat → confirm → `POST /bookings` → email notification triggered → confirmation shown in UI.

**Operator onboarding:** Register → admin approves → operator logs in → adds bus (linked to admin-created route) → admin approves bus → bus visible in search → operator adds schedules + seat layout → users can book.

**Admin approval chain:** Operator registers → admin sees pending list → approve/reject → same pattern for buses submitted by operators, and for refund requests.

**Seat layout:** `SeatLayouts` stores a JSONB config (e.g. `{rows: 10, cols: 4, aisleAfterCol: 2}`). The Angular `SeatLayoutModule` reads this config and renders the grid. Operators can choose an existing layout or create a new one.

**Pickup / drop logic:** When a user books a seat on a bus, the pickup point = operator's office address for that city, and the drop point = route's destination city address. No intermediate stops needed.

---

## Recommended npm / NuGet Packages

**Angular:** `@angular/material` (UI), `ngx-toastr` (notifications), a seat layout library like `ngx-seat-selector` or build a custom component.

**.NET:** `Microsoft.AspNetCore.Authentication.JwtBearer`, `BCrypt.Net-Next`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `SendGrid` or `MailKit`, `Serilog`.
