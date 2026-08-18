## Products
## 1. Common products page for all roles, specific rendering and api call is bound wrt conditional statements

## 2. Product-List page shows products with default variant (no individual card for a product). If user clicks a specific product, other variant details can be viewed. This is done for
- Faster page load
- Reduced server load
- Reduced bandwidth load
- Reduced client load

## 3.Infinite Loading
# `setupIntersectionObserver()` - Infinite Scroll

The `setupIntersectionObserver()` method sets up an **Intersection Observer** to detect when the user reaches the bottom of the product list. Instead of using a **"Load More"** button, the next page of products is automatically fetched when a hidden element (called the **sentinel**) becomes visible in the viewport.

## Code

```ts
setupIntersectionObserver() {
  if (typeof IntersectionObserver === 'undefined') {
    return;
  }

  this.observer = new IntersectionObserver(
    (entries) => {
      const entry = entries[0];

      if (
        entry.isIntersecting &&
        !this.loading() &&
        !this.loadingMore() &&
        this.hasNext
      ) {
        this.loadNextPage();
      }
    },
    { threshold: 0.1 }
  );

  if (this.sentinel) {
    this.observer.observe(this.sentinel.nativeElement);
  }
}
```

---

## Step-by-Step Explanation

### 1. Check Browser Support

```ts
if (typeof IntersectionObserver === 'undefined') {
  return;
}
```

* Ensures the browser supports the `IntersectionObserver` API.
* If not supported, the function exits without creating an observer.

---

### 2. Create an Observer

```ts
this.observer = new IntersectionObserver((entries) => {
    ...
});
```

Creates an `IntersectionObserver` object that continuously watches the target element (the sentinel).

Whenever the sentinel enters or leaves the viewport, the callback function is executed.

---

### 3. Get the Observed Entry

```ts
const entry = entries[0];
```

`entries` is an array containing information about observed elements.

Since only one element (`sentinel`) is being observed, we access the first entry.

---

### 4. Check Whether More Products Should Be Loaded

```ts
if (
    entry.isIntersecting &&
    !this.loading() &&
    !this.loadingMore() &&
    this.hasNext
)
```

All four conditions must be true:

| Condition              | Purpose                                                            |
| ---------------------- | ------------------------------------------------------------------ |
| `entry.isIntersecting` | Sentinel is visible in the viewport.                               |
| `!this.loading()`      | Initial product loading has completed.                             |
| `!this.loadingMore()`  | Prevents multiple API calls while another page is already loading. |
| `this.hasNext`         | Ensures another page of products exists.                           |

If all conditions are satisfied, the next page is requested.

```ts
this.loadNextPage();
```

---

### 5. Threshold

```ts
{ threshold: 0.1 }
```

The callback is triggered when **10% of the sentinel element becomes visible**.

A smaller threshold triggers earlier, while a larger threshold waits until more of the element is visible.

---

### 6. Start Observing the Sentinel

```ts
if (this.sentinel) {
    this.observer.observe(this.sentinel.nativeElement);
}
```

`@ViewChild` provides a reference to the sentinel element in the HTML.

The observer begins watching this element. Whenever the user scrolls and the sentinel becomes visible, the callback is executed.

---

## Overall Flow

```text
Page Loads
    │
    ▼
Observer starts watching the sentinel
    │
    ▼
User scrolls down
    │
    ▼
Sentinel enters the viewport
    │
    ▼
Conditions are checked
    │
    ▼
loadNextPage() is called
    │
    ▼
Next page of products is fetched
    │
    ▼
Products are appended to the existing list
```

## Why Use `IntersectionObserver`?

* Better performance than continuously listening to the `scroll` event.
* Reduces unnecessary calculations.
* Prevents repeated API calls using loading flags.
* Provides a smooth infinite scrolling experience.

