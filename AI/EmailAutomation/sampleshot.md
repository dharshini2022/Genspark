# Few-Shot Examples

## Example 1

### Input Client Text

"Hey, I need a simple mobile app for my food truck. Users should be able to see where the truck is parked today and look at the menu. I need it to load super fast because people look at it while standing on the sidewalk. I'm worried about what happens if our phone GPS drops out. Can we finish this in 2 weeks? Also, how do we update the menu?"

### Expected Output

### 1. Functional Requirements

* The system must display the current geographical location of the food truck on an interactive map.
* The system must display menu items including descriptions and prices.
* The platform must provide an administrative interface for updating menu information.

### 2. Non-functional Requirements

* Menu data and map information must load in under 2 seconds on mobile networks.
* The user interface must be optimized for mobile viewing.

### 3. Risks

* GPS signal interruption may result in inaccurate location information.
* A two-week delivery timeline introduces schedule risk.

### 4. Assumptions

* It is assumed that a mobile-first web application will be used.
* It is assumed that the operator possesses a device capable of transmitting location data.

### 5. Questions to Client

* Should menu updates occur manually or via POS integration?
* What should happen if GPS connectivity is lost?

---

## Example 2

### Input Client Text

"We need a portal where employees can upload receipts and managers approve reimbursements. Finance should receive reports every month. It should work for around 5,000 employees."

### Expected Output

### 1. Functional Requirements

* The system must allow employees to upload reimbursement receipts.
* The platform must support manager approval workflows.
* The system must generate monthly financial reports.

### 2. Non-functional Requirements

* The platform must support at least 5,000 concurrent users.
* Report generation should complete within acceptable processing times.
* Data transmission and storage must be secure.

### 3. Risks

* High user volume may impact performance.
* Incorrect approval workflows could delay reimbursements.

### 4. Assumptions

* It is assumed that employees authenticate using corporate credentials.
* It is assumed that finance reports are generated monthly.

### 5. Questions to Client

* Which authentication provider should be used?
* Are there reimbursement limits or approval hierarchies?
* Are audit logs required?
