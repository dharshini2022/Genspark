-- =============================================================
--  Bus Booking System – Sample Seed Data
--  Database: PostgreSQL | ORM: EF Core (enums stored as int)
--
--  Enum integer mappings:
--    UserRole:       Customer=0, Operator=1, Admin=2
--    ApprovalStatus: Pending=0, Approved=1, Disabled=2, Rejected=3
--    BusStatus:      Pending=0, Approved=1, Active=2, Down=3, Removed=4
--    BookingStatus:  Confirmed=0, Cancelled=1
--    PaymentStatus:  Pending=0, Success=1, Refunded=2
--
--  All passwords are BCrypt hashes:
--    Admin@123   → already seeded via HasData
--    Operator@123
--    Customer@123
-- =============================================================

BEGIN;

-- ─────────────────────────────────────────
-- 1. USERS  (id=1 is the admin seeded by HasData)
-- ─────────────────────────────────────────
INSERT INTO "Users" ("Id","Name","Email","Phone","PasswordHash","Role","CreatedAt","IsActive")
VALUES
  -- Operators
  (2, 'Rajesh Kumar',   'rajesh@srtravels.com',  '9876543210',
   '$2a$11$xKzRR3.Y1YxK7LRVBHpQAuaioN.d4sQMV1/0gI2t7fLKbH2qlCHay', 1,
   '2024-01-10 00:00:00 UTC', true),
  (3, 'Meena Sundar',   'meena@vipbus.com',      '9876500001',
   '$2a$11$xKzRR3.Y1YxK7LRVBHpQAuaioN.d4sQMV1/0gI2t7fLKbH2qlCHay', 1,
   '2024-01-12 00:00:00 UTC', true),
  -- Customers
  (4, 'Arjun Sharma',   'arjun@gmail.com',       '9123456789',
   '$2a$11$HvzVf6n6LWMzJJWzBL9a8eJH8v.tKNgA07N7K1gfv0D1D0jS0u9Vy', 0,
   '2024-02-01 00:00:00 UTC', true),
  (5, 'Priya Nair',     'priya@gmail.com',        '9234567890',
   '$2a$11$HvzVf6n6LWMzJJWzBL9a8eJH8v.tKNgA07N7K1gfv0D1D0jS0u9Vy', 0,
   '2024-02-05 00:00:00 UTC', true),
  (6, 'Karthik Raj',   'karthik@gmail.com',      '9345678901',
   '$2a$11$HvzVf6n6LWMzJJWzBL9a8eJH8v.tKNgA07N7K1gfv0D1D0jS0u9Vy', 0,
   '2024-02-10 00:00:00 UTC', true)
ON CONFLICT ("Id") DO NOTHING;

-- Update the sequence so next auto-increment starts after our inserted IDs
SELECT setval(pg_get_serial_sequence('"Users"', 'Id'), (SELECT MAX("Id") FROM "Users"));


-- ─────────────────────────────────────────
-- 2. BUS OPERATORS
-- ─────────────────────────────────────────
INSERT INTO "BusOperators" ("Id","UserId","CompanyName","GSTNumber","Status","ApprovedByAdminId","RegisteredAt")
VALUES
  (1, 2, 'SR Travels',   '33AABCU9603R1ZJ', 1, 1, '2024-01-11 00:00:00 UTC'),
  (2, 3, 'VIP Bus Lines','33BBBCU1234R1ZK', 1, 1, '2024-01-13 00:00:00 UTC')
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"BusOperators"', 'Id'), (SELECT MAX("Id") FROM "BusOperators"));


-- ─────────────────────────────────────────
-- 3. OPERATOR OFFICES
-- ─────────────────────────────────────────
INSERT INTO "OperatorOffices" ("Id","OperatorId","District","City","Address")
VALUES
  (1, 1, 'Chennai',    'Chennai',    'SR Travels, 45 Anna Salai, Teynampet, Chennai - 600018'),
  (2, 1, 'Coimbatore', 'Coimbatore', 'SR Travels, 12 Avinashi Road, Peelamedu, Coimbatore - 641004'),
  (3, 2, 'Chennai',    'Chennai',    'VIP Bus Lines, 88 Mount Road, Anna Nagar, Chennai - 600040'),
  (4, 2, 'Madurai',    'Madurai',    'VIP Bus Lines, 3 Periyar Bus Stand Road, Madurai - 625001')
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"OperatorOffices"', 'Id'), (SELECT MAX("Id") FROM "OperatorOffices"));


-- ─────────────────────────────────────────
-- 4. BUS ROUTES  (id=1,2 already seeded)
-- ─────────────────────────────────────────
INSERT INTO "Routes" ("Id","SourceCity","DestinationCity","CreatedByAdminId","CreatedAt","IsActive")
VALUES
  (3, 'Coimbatore', 'Madurai',   1, '2024-01-05 00:00:00 UTC', true),
  (4, 'Chennai',    'Bangalore', 1, '2024-01-05 00:00:00 UTC', true),
  (5, 'Madurai',    'Trichy',    1, '2024-01-05 00:00:00 UTC', true)
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"Routes"', 'Id'), (SELECT MAX("Id") FROM "Routes"));


-- ─────────────────────────────────────────
-- 5. BUS LAYOUTS  (id=1,2 already seeded)
-- ─────────────────────────────────────────
INSERT INTO "BusLayouts" ("Id","Name","Description","TotalRows","SeatsPerRow","HasUpperDeck","IsGlobal","LayoutJson","CreatedAt")
VALUES
  (3, 'Semi-Sleeper 2+2 (36 Seats)',
     '9 rows, 2 seats each side, semi-sleeper recline',
     9, 4, false, true,
     '{"rows":9,"cols":4,"aisle":2,"seats":[["1A","1B",null,"1C","1D"],["2A","2B",null,"2C","2D"],["3A","3B",null,"3C","3D"],["4A","4B",null,"4C","4D"],["5A","5B",null,"5C","5D"],["6A","6B",null,"6C","6D"],["7A","7B",null,"7C","7D"],["8A","8B",null,"8C","8D"],["9A","9B",null,"9C","9D"]]}',
     '2024-01-01 00:00:00 UTC')
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"BusLayouts"', 'Id'), (SELECT MAX("Id") FROM "BusLayouts"));


-- ─────────────────────────────────────────
-- 6. BUSES
-- ─────────────────────────────────────────
INSERT INTO "Buses" ("Id","OperatorId","BusName","RegistrationNumber","BusType","LayoutId","Status","ApprovedByAdminId","CreatedAt")
VALUES
  (1, 1, 'SR Express 01', 'TN01AB1001', 'AC',           1, 2, 1, '2024-01-15 00:00:00 UTC'),
  (2, 1, 'SR Express 02', 'TN01AB1002', 'Sleeper',      2, 2, 1, '2024-01-15 00:00:00 UTC'),
  (3, 2, 'VIP Deluxe 01', 'TN02CD2001', 'AC',           3, 2, 1, '2024-01-20 00:00:00 UTC'),
  (4, 2, 'VIP Sleeper 01','TN02CD2002', 'AC Sleeper',   2, 2, 1, '2024-01-20 00:00:00 UTC'),
  (5, 1, 'SR Non-AC 01',  'TN01AB1003', 'Non-AC',       1, 0, NULL,'2024-02-01 00:00:00 UTC')
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"Buses"', 'Id'), (SELECT MAX("Id") FROM "Buses"));


-- ─────────────────────────────────────────
-- 7. BUS SCHEDULES
-- ─────────────────────────────────────────
INSERT INTO "BusSchedules" ("Id","BusId","RouteId","DepartureTime","ArrivalTime","PricePerSeat","IsCancelled","CreatedAt")
VALUES
  -- SR Express 01: Chennai → Coimbatore (tomorrow 22:00)
  (1, 1, 1, (NOW() + INTERVAL '1 day')::date + TIME '22:00:00',
              (NOW() + INTERVAL '2 days')::date + TIME '05:30:00',
              650.00, false, NOW()),

  -- SR Express 01: Chennai → Coimbatore (day after tomorrow)
  (2, 1, 1, (NOW() + INTERVAL '2 days')::date + TIME '22:00:00',
              (NOW() + INTERVAL '3 days')::date + TIME '05:30:00',
              650.00, false, NOW()),

  -- SR Express 02 Sleeper: Chennai → Madurai
  (3, 2, 2, (NOW() + INTERVAL '1 day')::date + TIME '21:00:00',
              (NOW() + INTERVAL '2 days')::date + TIME '05:00:00',
              800.00, false, NOW()),

  -- VIP Deluxe: Chennai → Coimbatore
  (4, 3, 1, (NOW() + INTERVAL '1 day')::date + TIME '20:30:00',
              (NOW() + INTERVAL '2 days')::date + TIME '04:30:00',
              700.00, false, NOW()),

  -- VIP Sleeper: Chennai → Madurai
  (5, 4, 2, (NOW() + INTERVAL '1 day')::date + TIME '21:30:00',
              (NOW() + INTERVAL '2 days')::date + TIME '05:30:00',
              900.00, false, NOW()),

  -- SR Express 01: Chennai → Bangalore
  (6, 1, 4, (NOW() + INTERVAL '1 day')::date + TIME '23:00:00',
              (NOW() + INTERVAL '2 days')::date + TIME '06:00:00',
              750.00, false, NOW()),

  -- Past schedule (for booking history)
  (7, 1, 1, '2024-03-10 22:00:00 UTC',
             '2024-03-11 05:30:00 UTC',
              600.00, false, '2024-03-09 00:00:00 UTC')
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"BusSchedules"', 'Id'), (SELECT MAX("Id") FROM "BusSchedules"));


-- ─────────────────────────────────────────
-- 8. BOOKINGS  (using past schedule so no conflict)
-- ─────────────────────────────────────────
INSERT INTO "Bookings" ("Id","CustomerId","ScheduleId","TotalAmount","PlatformFee","Status","BookedAt","CancelledAt","RefundAmount","CancellationReason")
VALUES
  -- Arjun – confirmed booking on past schedule
  (1, 4, 7,  1260.00, 60.00, 0, '2024-03-09 10:00:00 UTC', NULL, NULL, NULL),
  -- Priya – confirmed booking on past schedule
  (2, 5, 7,   630.00, 30.00, 0, '2024-03-09 11:00:00 UTC', NULL, NULL, NULL),
  -- Karthik – cancelled booking on past schedule
  (3, 6, 7,  1260.00, 60.00, 1, '2024-03-09 12:00:00 UTC', '2024-03-10 08:00:00 UTC', 1008.00, 'Change of plans')
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"Bookings"', 'Id'), (SELECT MAX("Id") FROM "Bookings"));


-- ─────────────────────────────────────────
-- 9. BOOKING PASSENGERS
-- ─────────────────────────────────────────
INSERT INTO "BookingPassengers" ("Id","BookingId","SeatNumber","PassengerName","Age","Gender")
VALUES
  -- Booking 1 – Arjun (2 seats)
  (1, 1, '1A', 'Arjun Sharma',    28, 'Male'),
  (2, 1, '1B', 'Divya Sharma',    25, 'Female'),
  -- Booking 2 – Priya (1 seat)
  (3, 2, '2C', 'Priya Nair',      30, 'Female'),
  -- Booking 3 – Karthik (2 seats, cancelled)
  (4, 3, '3A', 'Karthik Raj',     32, 'Male'),
  (5, 3, '3B', 'Ananya Raj',      28, 'Female')
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"BookingPassengers"', 'Id'), (SELECT MAX("Id") FROM "BookingPassengers"));


-- ─────────────────────────────────────────
-- 10. PAYMENTS
-- ─────────────────────────────────────────
INSERT INTO "Payments" ("Id","BookingId","Amount","PaymentMethod","TransactionId","Status","PaidAt")
VALUES
  (1, 1, 1260.00, 'Dummy', 'TXN-20240309-001', 1, '2024-03-09 10:01:00 UTC'),
  (2, 2,  630.00, 'Dummy', 'TXN-20240309-002', 1, '2024-03-09 11:01:00 UTC'),
  (3, 3, 1260.00, 'Dummy', 'TXN-20240309-003', 2, '2024-03-09 12:01:00 UTC')
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"Payments"', 'Id'), (SELECT MAX("Id") FROM "Payments"));


-- ─────────────────────────────────────────
-- 11. PLATFORM SETTINGS (id=1-4 already seeded)
-- ─────────────────────────────────────────
INSERT INTO "PlatformSettings" ("Id","Key","Value","Description","UpdatedAt","UpdatedByAdminId")
VALUES
  (5, 'SeatBlockDurationMinutes', '5',
     'How long a seat is temporarily blocked during booking (in minutes)',
     '2024-01-01 00:00:00 UTC', 1),
  (6, 'MaxSeatsPerBooking', '10',
     'Maximum number of seats a single customer can book per transaction',
     '2024-01-01 00:00:00 UTC', 1)
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"PlatformSettings"', 'Id'), (SELECT MAX("Id") FROM "PlatformSettings"));

COMMIT;

-- ─────────────────────────────────────────
-- VERIFICATION QUERIES
-- ─────────────────────────────────────────
SELECT 'Users'             AS "Table", COUNT(*) AS "Rows" FROM "Users"
UNION ALL
SELECT 'BusOperators',       COUNT(*) FROM "BusOperators"
UNION ALL
SELECT 'OperatorOffices',    COUNT(*) FROM "OperatorOffices"
UNION ALL
SELECT 'Routes',             COUNT(*) FROM "Routes"
UNION ALL
SELECT 'BusLayouts',         COUNT(*) FROM "BusLayouts"
UNION ALL
SELECT 'Buses',              COUNT(*) FROM "Buses"
UNION ALL
SELECT 'BusSchedules',       COUNT(*) FROM "BusSchedules"
UNION ALL
SELECT 'Bookings',           COUNT(*) FROM "Bookings"
UNION ALL
SELECT 'BookingPassengers',  COUNT(*) FROM "BookingPassengers"
UNION ALL
SELECT 'Payments',           COUNT(*) FROM "Payments"
UNION ALL
SELECT 'PlatformSettings',   COUNT(*) FROM "PlatformSettings"
ORDER BY "Table";
