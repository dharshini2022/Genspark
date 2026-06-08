# Angular Parent-Child Component Communication

This document explains the parent-to-child and child-to-parent communication patterns implemented between the **Products** (parent) and **Product** (child) components in this project.

---

## 1. Parent-to-Child Communication (Passing Data Down)

The parent component passes data down to the child component using property binding.

### Child Component Configuration
* **File:** [product.ts](file:///Users/dharshinik/Desktop/Presidio/Genspark/Angular/Learn/src/app/product/product.ts)
* **Mechanism:** The child component uses Angular's modern signal-based `input()` function:
  ```typescript
  product = input<ProductModel>();
  ```
  *(Note: There is also a commented-out option showing the traditional `@Input()` decorator pattern: `// @Input() product:ProductModel = {} as ProductModel;`)*

* **Template Usage ([product.html](file:///Users/dharshinik/Desktop/Presidio/Genspark/Angular/Learn/src/app/product/product.html)):**
  The template retrieves values by calling the input signal:
  ```html
  <img class="card-img-top" [src]="product()?.thumbnail" alt="Card image cap">
  <h5 class="card-title">{{product()?.title}}</h5>
  ```

### Parent Component Usage
* **File:** [products.html](file:///Users/dharshinik/Desktop/Presidio/Genspark/Angular/Learn/src/app/products/products.html)
* **Mechanism:** The parent component renders the child element and binds data using the `[product]` property:
  ```html
  @for (product of products(); track product.id) 
  {
       <app-product [product]="product" (buy)="handleBuy($event)"/>
  }
  ```

---

## 2. Child-to-Parent Communication (Emitting Events Up)

The child component communicates user actions back up to the parent using an `@Output()` event emitter.

### Child Component Configuration
* **File:** [product.ts](file:///Users/dharshinik/Desktop/Presidio/Genspark/Angular/Learn/src/app/product/product.ts)
* **Mechanism:** The child component declares an event emitter decorated with `@Output()`:
  ```typescript
  @Output() buy = new EventEmitter<ProductModel>();
  ```

* **Template Usage ([product.html](file:///Users/dharshinik/Desktop/Presidio/Genspark/Angular/Learn/src/app/product/product.html)):**
  A button click triggers the component's internal `handleClick()` handler:
  ```html
  <a (click)="handleClick()" class="btn btn-primary">Buy for ${{product()?.price}}</a>
  ```

* **Emitting the Event:**
  Inside `handleClick()`, the component emits the current product data:
  ```typescript
  handleClick(){
    this.buy.emit(this.product());
  }
  ```

### Parent Component Handling
* **Template Usage ([products.html](file:///Users/dharshinik/Desktop/Presidio/Genspark/Angular/Learn/src/app/products/products.html)):**
  The parent listens for the custom `(buy)` event and maps it to a method, passing the event payload as `$event`:
  ```html
  <app-product [product]="product" (buy)="handleBuy($event)"/>
  ```

* **Parent Class Handler ([products.ts](file:///Users/dharshinik/Desktop/Presidio/Genspark/Angular/Learn/src/app/products/products.ts)):**
  The parent handles the event payload in the `handleBuy` method:
  ```typescript
  handleBuy(product: ProductModel){
    alert(`You bought ${product.title} for $${product.price}`);
    this.cart().push(product);
  }
  ```
