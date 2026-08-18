# Understanding Refresh Token Rotation (A Guide for Novices)

Welcome! If you are new to web security, this guide will help you understand how **Access Tokens**, **Refresh Tokens**, and **Automatic Token Rotation** work together to keep users logged in securely without manual hassle.

---

## 1. The Core Analogy: Hotel Room Access

Imagine you are staying at a high-security hotel:

* **The Access Token (The Key Card):** 
  When you check in, you are given a temporary plastic key card. It lets you open your room door. For security, it expires every **30 minutes**. If someone steals your key card, they can only access your room for a maximum of 30 minutes.
* **The Refresh Token (Your Reservation Confirmation):** 
  This is a paper reservation slip signed by the hotel manager. You keep it locked safely in your pocket. When your key card stops working after 30 minutes, you show this reservation slip to the front desk, and they hand you a new key card.
* **Token Rotation (Key Card & Slip Exchange):**
  To make things even safer, every time you show your reservation slip to get a new key card, the front desk **destroys your old reservation slip** and gives you a brand-new one along with your new key card. If a thief somehow stole your old reservation slip, it is now useless because it has already been deactivated!

---

## 2. Why Do We Need "Rotation"?

If an attacker manages to steal a user's Refresh Token, they could theoretically keep generating new Access Tokens forever, keeping themselves logged in as the victim indefinitely.

With **Refresh Token Rotation**:
1. Every time a refresh token is used, it is **invalidated (revoked)**.
2. A brand-new refresh token is issued in its place.
3. If an attacker steals a refresh token and tries to use it *after* the legitimate user already used it, the server detects that the old token was used twice. The server realizes a theft has occurred, invalidates the entire family of tokens, and forces everyone (including the attacker) to log in again.

---

## 3. The Step-by-Step Technical Workflow

Here is how the browser (Angular frontend) and the server (ASP.NET Core backend) work together automatically when a token expires.

### A. Sequence Diagram (Time-based Interactions)

```mermaid
sequenceDiagram
    autonumber
    actor User as Browser (Angular Client)
    participant Server as Web Server (API Backend)
    database DB as Database
    
    User->>Server: 1. Send API Request with Header (Authorization: Bearer <Access Token>)
    Server-->>User: 2. Respond with 401 Unauthorized (Access Token is expired)
    Note over User: Interceptor locks the queue & stops new requests
    User->>Server: 3. Send POST /refresh request containing { expiredAccessToken, refreshToken } in body
    Note over Server: Decodes expired Access Token to get User ID
    Server->>DB: 4. Check active refresh tokens & mark old one as Revoked
    Server->>DB: 5. Save new generated Refresh Token (BCrypt Hashed)
    Server-->>User: 6. Respond 200 OK with new { accessToken, refreshToken } in body
    Note over User: Save tokens to Local Storage & retry original request
    User->>Server: 7. Resend original request with new Access Token header (Succeeds!)
```

### B. Decision Workflow Diagram (Logical Decisions)

```mermaid
graph TD
    Start[API Request Triggered] --> Interceptor{Is AccessToken Expired?}
    Interceptor -->|No| SendRequest[Send Request with Bearer Token Header]
    Interceptor -->|Yes| CheckRefresh{Is Refresh In Progress?}
    
    SendRequest --> Response{Is Response 401 Unauthorized?}
    Response -->|No (200 OK)| Success[Success]
    Response -->|Yes (401)| CheckRefresh
    
    CheckRefresh -->|Yes| QueueRequest[Queue Request in Waitlist]
    CheckRefresh -->|No| TriggerRefresh[Set isRefreshing = true & Call /refresh]
    
    TriggerRefresh --> RefreshResponse{Did /refresh Succeed?}
    RefreshResponse -->|Yes| ReleaseQueue[Set isRefreshing = false, Save tokens, Release Waitlist & Retry Requests]
    RefreshResponse -->|No| ClearSession[Clear Local Storage, call logout & redirect to Login]
    
    QueueRequest --> WaitRefresh[Wait for release signal]
    WaitRefresh --> RetryRequest[Retry original request with new token]
    ReleaseQueue --> RetryRequest
    RetryRequest --> Success
```

### The Role of the Angular HTTP Interceptor
The **Interceptor** acts like a traffic controller on the frontend. When a request fails because the token is expired:
* It checks if a refresh is already happening.
* If **not**, it starts the refresh and pauses all other outgoing requests.
* If a refresh **is** already happening, it holds all new requests in a queue (a waitlist) until the new tokens arrive. This prevents sending duplicate refresh requests.

---

## 4. How Timing Scenarios Are Handled

Here is what happens during three common timing conditions:

### Scenario A: Request sent just before the token expires
* **What happens:** The token is still valid when it reaches the server. 
* **Result:** The server processes it successfully (`200 OK`). No refresh is triggered.

### Scenario B: Request is sent, but expires in transit
* **What happens:** The token was valid when you clicked, but expired by the millisecond it reached the server.
* **Result:** The server returns `401 Unauthorized`. The interceptor catches this, pauses further requests, updates the tokens by calling `/refresh`, and automatically retries the request. The user notices no delay.

### Scenario C: Multiple requests fail at the same time (The Concurrency Race)
* **What happens:** You load a dashboard that triggers 5 API calls at once, and the access token has expired.
* **Result:** 
  1. The first call to fail (e.g. Call 1) triggers the `/refresh` call and locks the door (`isRefreshing = true`).
  2. The other 4 calls also fail with `401`, but instead of triggering 4 more `/refresh` calls, the interceptor puts them in a **queue (waitlist)**.
  3. Once Call 1's refresh finishes and receives the new tokens, the interceptor releases the queued calls and retries them. They all succeed using the new token.

---

## 5. Token Storage in Local Storage

In this codebase, we use **Option B (Local Storage & Bearer Headers)** to store tokens:

* **What it means:** Tokens are stored directly in the browser's `localStorage` and sent manually in the `Authorization` header.
* **Role-Based Storage Keys:**
  For extra safety and cleanly separated user sessions, we store the access tokens in different keys based on the user's active role:
  * Customer role: `customer_token`
  * Vendor role: `vendor_token`
  * Admin role: `admin_token`
  * Single rotated refresh key: `refresh_token`
* **Why it's robust:** 
  It completely avoids standard browser cookie constraints during cross-site requests, making it ideal for API-first applications and local development where the client and backend run on different ports.

---

## 6. Why We Migrated Away from HttpOnly Cookies: CORS & SameSite lax Rules

While HttpOnly cookies offer great protection against XSS (Cross-Site Scripting), they introduce significant friction in multi-port and multi-origin local environments:

1. **SameSite Lax Restrictions:** 
   Modern browsers restrict cookies marked with `SameSite=Lax` (the default) from being sent on cross-site AJAX requests. If your frontend runs on `http://localhost:4200` and your backend runs on `https://localhost:7195`, they are considered cross-site due to the scheme difference (`http` vs `https`). The browser blocks the cookie, causing `/refresh` to throw `Refresh token is missing`.
2. **The Secure Attribute Conflict:** 
   If cookies are set as `Secure = true` (which is required if you want to use `SameSite=None` to allow cross-site requests), the browser will refuse to save or send them over unencrypted `http://localhost:5288` connections.
3. **The Solution:** 
   By switching to `localStorage` and attaching the token via `Authorization` headers, we bypass cookie-jar restrictions completely. The application works out-of-the-box in both HTTP and HTTPS local configurations, and handles the WebSocket connection for SignalR (`/notificationHub`) securely by appending the token to the query string via the client connection's `accessTokenFactory`.


