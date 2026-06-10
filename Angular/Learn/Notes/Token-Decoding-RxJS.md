# Token Decoding and State Management with RxJS

This document explains the workflow of extracting a user's name from a JWT token upon login, managing its state using RxJS, and displaying it in the main application component.

---

## Workflow Overview

When a user successfully logs in, the API returns a JWT token. The application extracts the user's name from the token's payload and propagates it across the application using an RxJS `Subject` and Angular Signals.

### 1. Triggering the Update on Login
* **File:** [login.ts](file:///Users/dharshinik/Desktop/Presidio/Genspark/Angular/Learn/src/app/login/login.ts)
* **Process:** After a successful login API call, the token is stored in the browser's `sessionStorage`. Then, the `changeUsername()` function is called to initiate the extraction process.
  ```typescript
  sessionStorage.setItem('token', response.token);
  // ...
  changeUsername();
  ```

### 2. Extracting Data and Emitting via RxJS Subject
* **File:** [auth.operator.ts](file:///Users/dharshinik/Desktop/Presidio/Genspark/Angular/Learn/src/app/rxjs/auth.operator.ts)
* **Process:** The `changeUsername` function retrieves the token, decodes the payload, extracts the specific claim containing the username, and pushes the new value into the `usernameSubject`.
  ```typescript
  export const usernameSubject = new Subject<string>();

  export const changeUsername = () => {
      const token = sessionStorage.getItem("token");
      if (token) {
          // Decode the base64 URL payload
          const payload = JSON.parse(atob(token.split(".")[1]));
          // Extract the specific claim
          const name = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"];
          if (name) {
              // Emit the new username
              usernameSubject.next(name);
          }
      }
  }
  ```

### 3. Subscribing to the Subject in the App Component
* **File:** [app.ts](file:///Users/dharshinik/Desktop/Presidio/Genspark/Angular/Learn/src/app/app.ts)
* **Process:** The root `App` component initializes a `username` signal. In its constructor, it subscribes to the `usernameSubject`. Whenever a new name is emitted, the signal is updated.
  ```typescript
  username = signal('Guest');
  
  constructor(){
    usernameSubject.subscribe({
      next: (un) => {
        this.username.set(un); // Update the signal
      }
    })
  }
  ```

### 4. Displaying the Data in the Template
* **File:** [app.html](file:///Users/dharshinik/Desktop/Presidio/Genspark/Angular/Learn/src/app/app.html)
* **Process:** The template binds to the `username` signal and updates automatically whenever the signal's value changes.
  ```html
  <h1 align="center"> Hello {{username()}} </h1>
  ```
