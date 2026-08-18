# Learning: Custom Header & Role-Based HttpOnly Cookies

This document explains the architecture and design pattern of storing role-based JWT tokens in **HttpOnly** cookies and routing requests using a custom HTTP header.

---

## 1. The Core Architecture & Challenge

### Problem Statement
Standard authentication systems store the access token in a single HttpOnly cookie (e.g., `jwt_token`) or in local/session storage. 
1. Storing tokens in client storage (`localStorage` or `sessionStorage`) exposes them to theft via Cross-Site Scripting (XSS) attacks.
2. Storing the token in a single HttpOnly cookie is secure against XSS, but it makes it difficult to support **concurrent role sessions** (e.g., having one browser tab acting as a `Customer`, another as a `Vendor`, and another as an `Admin`) because logging into a new role overwrites the active token cookie.

### The Solution: Role-Based Cookies + Custom Header
By separating the cookies into role-specific names:
- `AdminToken`
- `VendorToken`
- `CustomerToken`

We allow all three to coexist in the browser's cookie jar. However, since the browser automatically sends **all matching cookies** on every request to the API domain, the backend needs a way to identify which cookie to authenticate against.
To solve this, the frontend adds a custom header (`X-Role`) on each request indicating the context of the active tab. The backend reads this header to determine which cookie to extract the token from.

---

## 2. Implementation Overview

### Client-Side (Angular Interceptor)
The frontend interceptor retrieves the current active role from session storage and adds the custom `X-Role` header to all outgoing requests.

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const role = sessionStorage.getItem('role');
  const headers: { [key: string]: string } = {};
  
  if (role) {
    // Format to match backend casing (e.g., Customer, Vendor, Admin)
    const formattedRole = role.charAt(0).toUpperCase() + role.slice(1).toLowerCase();
    headers['X-Role'] = formattedRole;
  }

  const cloned = req.clone({
    withCredentials: true, // Crucial for sending HttpOnly cookies
    setHeaders: headers
  });
  
  return next(cloned);
};
```

### Server-Side (ASP.NET Core JWT Middleware)
The backend middleware intercepts incoming requests and extracts the appropriate cookie token dynamically.

```csharp
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var token = string.Empty;
        var roleHeader = context.Request.Headers["X-Role"].ToString();

        // 1. Identify which role-specific cookie to read based on X-Role header
        if (!string.IsNullOrEmpty(roleHeader))
        {
            token = context.Request.Cookies[$"{roleHeader}Token"];
        }

        // 2. Fallback to priority-ordered cookies if no header is present
        if (string.IsNullOrEmpty(token))
        {
            token = context.Request.Cookies["jwt_token"] 
                    ?? context.Request.Cookies["CustomerToken"] 
                    ?? context.Request.Cookies["VendorToken"] 
                    ?? context.Request.Cookies["AdminToken"];
        }

        if (!string.IsNullOrEmpty(token))
        {
            context.Token = token;
        }
        return Task.CompletedTask;
    }
};
```

---

## 3. Gotcha: Duplicated Tabs & State Caching Race Condition

### The Problem
When a user duplicates a browser tab, the browser automatically copies the `sessionStorage` contents of the source tab into the newly created tab. This creates a state persistence race condition:
1. Suppose Tab 1 is logged in as a **Customer** (so its `sessionStorage` holds `role = "Customer"`).
2. The user duplicates Tab 1 to open Tab 2, and logs in as a **Vendor**.
3. The POST request to `/Auth/login` successfully authenticates the Vendor and sets the `VendorToken` cookie in the browser.
4. Immediately after login, the frontend invokes `fetchCurrentUserDetails()` to get `/User/my-token-payload`.
5. Since `fetchCurrentUserDetails()` goes through the interceptor, the interceptor reads the *copied* role from session storage (`role = "Customer"`), and attaches the request header `X-Role: Customer`.
6. The backend receives the request, sees `X-Role: Customer`, reads the existing `CustomerToken` cookie, and returns the profile details of the **Customer** account.
7. The frontend is updated with the Customer profile and redirects the Vendor tab back to `/customer-home`.

### The Resolution
To prevent this, the client must capture and update the new role in `sessionStorage` **before** fetching the user details. We added a `tap` operator in the `login()` service stream:

```typescript
  login(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, data).pipe(
      tap((response: any) => {
        sessionStorage.removeItem('cart_count');
        sessionStorage.removeItem('wishlist_count');
        if (response && response.role) {
          // Immediately update sessionStorage with the new role
          sessionStorage.setItem('role', response.role);
        }
      }),
      switchMap(() => {
        // Subsequent request now carries the correct X-Role header
        return this.fetchCurrentUserDetails();
      })
    );
  }
```

---

## 4. Key Benefits

1. **Defense in Depth (XSS Protection)**: Access tokens are stored in `HttpOnly` and `Secure` cookies, preventing malicious scripts from reading them even if an XSS vulnerability exists on the frontend.
2. **Concurrent Tab Support**: Users (especially developers or multi-role users) can seamlessly perform actions as a Customer, Vendor, or Admin on different tabs within the same browser session without their sessions conflicting.
3. **Decoupled Roles**: The backend can route and enforce role authorization cleanly at the endpoint level (`[Authorize(Roles = "Admin")]`), knowing that the middleware has already verified the correct cookie context.
4. **Graceful Fallback**: If a request comes from an external client (like Swagger or Postman) that does not send the `X-Role` header, the backend falls back to checking the default cookies in order of priority.
