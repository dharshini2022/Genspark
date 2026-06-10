# Angular Routing Documentation

## 1. Definition
Routing in Angular allows users to navigate from one view or component to another as they interact with the application. In a Single Page Application (SPA), the Angular Router manages state transitions, updates the browser's URL history, and swaps out components dynamically without requiring a full page reload from the server.

## 2. Code Explanation
Based on the current implementation in the `Learn` project, here is how routing is configured and used:

### Route Definitions (`src/app/app.routes.ts`)
The `Routes` array maps specific URL paths to the components that should be displayed.

```typescript
import { Routes } from '@angular/router';
import { Customer } from './customer/customer';

export const routes: Routes = [
    { path: 'home', component: Customer },
    { path: 'products', loadComponent: () => import('./products/products').then(m => m.Products) },
];
```
- **Eager Loading**: `{ path: 'home', component: Customer }` tells the router that when the URL is `/home`, it should immediately instantiate and display the `Customer` component. This component is bundled with the main application load.
- **Lazy Loading**: `{ path: 'products', loadComponent: ... }` uses a function that dynamically imports the `Products` component only when the user navigates to the `/products` path. This reduces the initial bundle size and improves load performance.

### Router Provider Configuration (`src/app/app.config.ts`)
The routes are provided to the entire application using the `provideRouter` function within the main application configuration.

```typescript
import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    // ... other providers
  ]
};
```

### Router Outlet (`src/app/app.html` & `src/app/app.ts`)
The `<router-outlet>` directive serves as a dynamic placeholder. This is where the Router inserts the component that matches the current route.

In `app.html`:
```html
<app-navigation></app-navigation>
<hr/>
<router-outlet></router-outlet>
```

In `app.ts`, `RouterOutlet` must be imported in the standalone component to use the directive in its template:
```typescript
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [/*...*/ RouterOutlet /*...*/],
  // ...
})
export class App { ... }
```

### Router Links (`src/app/navigation/navigation.html`)
Navigation links use the `routerLink` directive instead of the standard HTML `href` attribute. This allows the Angular Router to handle navigation internally rather than letting the browser make a new HTTP request to the server.

```html
<ul class="navbar-nav">
  <li class="nav-item active">
    <a class="nav-link" routerLink="home">Home</a> 
  </li>
  <!-- ... -->
  <li class="nav-item">
    <a class="nav-link" routerLink="products">Products</a>
  </li>
</ul>
```

## 3. Workflow
The routing process in the application operates through the following steps:

1. **User Interaction**: A user clicks on a navigation link in the UI, such as the "Products" link within the `<app-navigation>` component, which has the `routerLink="products"` directive attached.
2. **Event Interception**: The Angular Router intercepts the click event. It prevents the default browser behavior of navigating and refreshing the page.
3. **URL Update**: The Router updates the browser's address bar to reflect the new path (e.g., changing the URL to `/products`) and pushes a new state to the browser's history API.
4. **Route Matching**: The Router consults its configuration (from `app.routes.ts`) and searches for a `Route` object whose `path` matches the current URL.
5. **Component Activation**:
   - If the route specifies a `component` (like the `home` route), the Router prepares to instantiate it.
   - If the route specifies a `loadComponent` function (like the `products` route), the Router first fetches the required Javascript module asynchronously over the network, and then prepares to instantiate the component.
6. **Rendering**: The Router instantiates the matched component and inserts its view into the DOM right below the `<router-outlet>` directive in `app.html`. If there was a previously rendered component in the outlet, it is destroyed before the new one is displayed.

## 4. Parent-Child Routing
Angular allows nesting routes to create complex, hierarchical layouts. A parent component can have its own `<router-outlet>` to display child components based on the URL.

In `src/app/app.routes.ts`:
```typescript
{ 
    path: 'account', 
    component: Account, 
    children: [
        { path: 'transaction/:accNum', component: Transaction }
    ]
}
```
Here, navigating to `/account/transaction/123` will load the `Account` component into the main `<router-outlet>`, and then load the `Transaction` component into the `Account` component's own `<router-outlet>`.

## 5. Parameterized Routing
Parameterized routing allows passing dynamic data in the URL.
In the route definition `{ path: 'transaction/:accNum', component: Transaction }`, `:accNum` is a dynamic route parameter.

To retrieve this parameter in the component, we inject `ActivatedRoute`:
```typescript
import { ActivatedRoute } from '@angular/router';

export class Transaction {
  fromaccountNumber: string = '';
  constructor(private activeRoute: ActivatedRoute) {
    // Snapshot way to retrieve the parameter
    this.fromaccountNumber = this.activeRoute.snapshot.params['accNum'];
  }
}
```
This is useful for public data or IDs that can be safely exposed in the URL.

## 6. Protected Routing: State & History API
When passing sensitive data (like an account number or transaction ID), exposing it in the URL via parameterized routing is not secure. Instead, we can use the Router's `state` object, which utilizes the browser's History API. The data is passed in memory and does not appear in the URL.

### Workflow:
1. **Navigate and Pass State**: In the parent component (`Account`), use `Router.navigate` and provide a state object.
```typescript
import { Router } from '@angular/router';

// ...
constructor(private router: Router) {}

handleSendMoneyClick(){
  // state makes the accountNumber invisible in the URL
  this.router.navigate(['/account/transaction'], { state: { accNum: this.accountDetails.accountNumber } });
}
```

2. **Retrieve State**: In the child component (`Transaction`), use `history.state` to retrieve the passed data.
```typescript
export class Transaction {
  fromaccountNumber: string = '';
  
  constructor() {
    // Retrieve the account number from the history state
    this.fromaccountNumber = history.state.accNum || '';
  }
}
```
*Note: Using this approach keeps the URL clean and prevents sensitive data from being visible in browser history or server logs.*
