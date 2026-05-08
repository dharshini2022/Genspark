# 📦 Database Design – Bus Booking System

This section explains the purpose of each table and how they relate to each other within the system.

---

## 🧩 1. Users

**Purpose:**
Stores authentication and identity details for all system roles (Customer, Operator, Admin).

**Key Fields:**

* `id`
* `name`, `email`, `phone`
* `password_hash`
* `role`
* `is_active`

**Relationships:**

* One-to-One → `BusOperators`
* One-to-Many → `Bookings`
* One-to-Many → `SeatBlocks`

---

## 🧩 2. BusOperators

**Purpose:**
Stores operator-specific business information.

**Key Fields:**

* `user_id`
* `company_name`
* `gst_number`
* `status` (approval status)

**Relationships:**

* Belongs to → `Users`
* One-to-Many → `Buses`
* One-to-Many → `OperatorOffices`

---

## 🧩 3. OperatorOffices

**Purpose:**
Stores office locations of bus operators (used as pickup points).

**Key Fields:**

* `operator_id`
* `district`, `city`, `address`

**Relationships:**

* Belongs to → `BusOperators`

---

## 🧩 4. BusRoutes

**Purpose:**
Defines available routes (source → destination), managed by Admin.

**Key Fields:**

* `source_city`
* `destination_city`
* `is_active`

**Relationships:**

* One-to-Many → `BusSchedules`

---

## 🧩 5. BusLayouts

**Purpose:**
Defines seat structure templates for buses.

**Key Fields:**

* `total_rows`, `seats_per_row`
* `has_upper_deck`
* `layout_json`

**Relationships:**

* One-to-Many → `Buses`

---

## 🧩 6. Buses

**Purpose:**
Represents a physical bus owned by an operator.

**Key Fields:**

* `operator_id`
* `bus_name`
* `registration_number`
* `bus_type`
* `layout_id`
* `status`

**Relationships:**

* Belongs to → `BusOperators`
* Belongs to → `BusLayouts`
* One-to-Many → `BusSchedules`

---

## 🧩 7. BusSchedules

**Purpose:**
Represents a scheduled trip (bus + route + time).

**Key Fields:**

* `bus_id`
* `route_id`
* `departure_time`, `arrival_time`
* `price_per_seat`

**Relationships:**

* Belongs to → `Buses`
* Belongs to → `BusRoutes`
* One-to-Many → `Bookings`
* One-to-Many → `SeatBlocks`

---

## 🧩 8. Bookings

**Purpose:**
Stores booking transactions made by customers.

**Key Fields:**

* `customer_id`
* `schedule_id`
* `total_amount`
* `status`
* `booked_at`

**Relationships:**

* Belongs to → `Users`
* Belongs to → `BusSchedules`
* One-to-Many → `BookingPassengers`
* One-to-One → `Payments`

---

## 🧩 9. BookingPassengers

**Purpose:**
Stores passenger details and assigned seats within a booking.

**Key Fields:**

* `booking_id`
* `seat_number`
* `passenger_name`, `age`, `gender`

**Relationships:**

* Belongs to → `Bookings`

---

## 🧩 10. Payments

**Purpose:**
Handles payment transactions for bookings.

**Key Fields:**

* `booking_id`
* `amount`
* `payment_method`
* `transaction_id`
* `status`

**Relationships:**

* One-to-One → `Bookings`

---

## 🧩 11. SeatBlocks

**Purpose:**
Temporarily locks seats during booking to prevent double booking.

**Key Fields:**

* `schedule_id`
* `seat_number`
* `blocked_by_user_id`
* `expires_at`

**Relationships:**

* Belongs to → `BusSchedules`
* Belongs to → `Users`

---

## 🧩 12. PlatformSettings

**Purpose:**
Stores configurable platform-level settings.

**Key Fields:**

* `key`, `value`
* `description`

**Relationships:**

* Updated by → `Users (Admin)`

---

# 🔗 System Flow Overview

### 🧭 Booking Process:

1. User searches buses
   → `BusRoutes → BusSchedules → Buses`

2. User selects a bus
   → Fetch `BusLayouts` for seat structure

3. Seat selection
   → `SeatBlocks` temporarily locks seats

4. Booking confirmation
   → `Bookings` + `BookingPassengers`

5. Payment
   → `Payments`

---

# 🧠 Relationship Summary

Users
├── BusOperators
│   ├── Buses
│   │   ├── BusSchedules
│   │   │   ├── Bookings
│   │   │   │   ├── BookingPassengers
│   │   │   │   └── Payments
│   │   │   └── SeatBlocks
│   │   └── BusLayouts
│   └── OperatorOffices
└── Bookings

---

# ⚠️ Key Design Notes

* Seat blocking prevents double booking conflicts
* Layouts are reusable via JSON configuration
* Routes are admin-controlled
* Payments are strictly linked to bookings
* Platform settings allow dynamic configuration

---

# 🚀 Conclusion

The schema is designed to be:

* Scalable
* Normalized
* Suitable for real-world booking workflows

The core logic revolves around:
👉 **BusSchedules + Seat Management + Booking Flow**
