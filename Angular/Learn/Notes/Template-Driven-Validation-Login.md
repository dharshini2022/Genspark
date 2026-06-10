# Template-Driven Validation in Angular: Login Component

## Definition

**Template-Driven Forms** in Angular rely primarily on directives in the template (HTML) to create and manage the underlying form instance. In this approach, most of the logic related to form controls, validation, and submission resides directly in the HTML template, keeping the component class relatively simple. Angular automatically creates an internal `NgForm` instance and `FormControl` instances for elements bound with `ngModel`.

In the `Login` component, we use template-driven validation to ensure user input meets specific criteria (e.g., required fields, minimum length) before the form is considered valid.

---

## Workflow

1. **Import `FormsModule`:** To use template-driven forms, the `FormsModule` must be imported in the component or module.
2. **Two-Way Data Binding (`[(ngModel)]`):** We bind the HTML input elements directly to the data model in the component using `[(ngModel)]`.
3. **Template Reference Variables (`#var="ngModel"`):** By exporting the `ngModel` directive to a local template variable (e.g., `#username="ngModel"`), we can access the control's state (validity, touched, errors) directly within the template.
4. **Validation Directives:** We add HTML5 validation attributes like `required` and `minlength` to the inputs. Angular directives translate these into validation rules.
5. **Conditional Error Display:** We use Angular control flow block (`@if`) to check if a control is invalid and has been touched before displaying the appropriate error message based on the specific validation error (`username.errors?.['required']`).

---

## Code Explanation

### 1. Component Setup (`login.ts`)

```typescript
import { Component, signal } from '@angular/core';
import { LoginModel } from './Model/login.model';
import { FormsModule } from '@angular/forms'; // 1. Important for template driven forms
// ... other imports

@Component({
  selector: 'app-login',
  imports: [FormsModule], // 2. FormsModule is registered here
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  // 3. Data model signal that will be bound to the form inputs
  loginModel = signal(new LoginModel()); 
  progress = signal(false);
  // ...
}
```
**Explanation:**
* The component imports `FormsModule` to enable template-driven forms functionality (`ngModel`).
* It exposes a data model `loginModel` (as a signal) that holds the `username` and `password` properties. This model is updated automatically as the user types.

### 2. Template and Validation (`login.html`)

```html
<div>
    <label for="username">Username:</label>
    <input type="text" id="username" 
    [(ngModel)]="loginModel().username" 
    name="username" #username="ngModel" 
     minlength="3" required>
    
    @if (username.touched && username.invalid) {
        <div class="alert alert-danger">
           @if (username.errors?.['required']) {
            <span>Username is required.</span>
           }
           @if (username.errors?.['minlength']) {
            <span>Username must be at least 3 characters long.</span>
           }
        </div>
    }
</div>
```
**Explanation:**
* `name="username"`: The `name` attribute is strictly required when using `ngModel` within a form or for validation to register the control with the form instance.
* `[(ngModel)]="loginModel().username"`: This establishes two-way data binding. Whatever the user types updates `loginModel().username`, and any changes to the model reflect in the input.
* `#username="ngModel"`: This is a **Template Reference Variable**. We assign the `ngModel` directive instance to the local variable `#username`. This allows us to check properties like `username.invalid`, `username.touched`, and `username.errors`.
* `required` and `minlength="3"`: These are HTML5 validation attributes. Angular uses them to perform validation behind the scenes.
* `@if (username.touched && username.invalid)`: This condition ensures that validation errors are only shown *after* the user has interacted with the input (`touched`) and the input is currently `invalid`.
* `@if (username.errors?.['required'])`: We check the specific error object. If the `required` validation failed, we show the "Username is required" message. The same logic applies to `minlength`.

### 3. Password Input

```html
<div>
    <label for="password">Password:</label>
    <input type="password" id="password" [(ngModel)]="loginModel().password" name="password" #password="ngModel" required>
</div>
```
**Explanation:**
* The password field functions similarly. It uses two-way binding to `loginModel().password` and sets `#password="ngModel"` to track its state, along with the `required` validator. (Though currently, explicit error messages aren't displayed for password in the snippet, it tracks validity under the hood).
