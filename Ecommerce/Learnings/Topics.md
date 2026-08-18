## ngOnInit() = Angular Lifecycle Hooks
- It gets executed immediately after the component call
```
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {

  users: User[] = [];

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    console.log('Component initialized');
    this.userService.getUsers().subscribe(data => {
      this.users = data;
    });
  }

}
```
**Purpose**
-> used for backend API calls

| Constructor                                   | `ngOnInit()`                                   |
| ----------------------------------------------| ---------------------------------------------- |
| - Called when the object is created           | - Called after Angular initializes the component |
| - Used for dependency injection               | - Used for initialization logic                  |
| - Runs before `@Input()` values are available | - `@Input()` values are available                |
| - Avoid API calls here                        | - API calls are commonly placed here             |

---
## ElementRef
- refers to an html element in ts
- Angular restricts direct manipulation of HTML element in DOM
- ElementRef is a wrapper around the html element, allowing for manipulation.
- Here, nativeElement Refers to the particular element of ElementRef
---

## TS Array Methods
| Method            | Definition                                                                | Short Example                           | Output              |
| ----------------- | ------------------------------------------------------------------------- | --------------------------------------- | ------------------- |
| `map()`           | Transforms every element and returns a new array.                         | `["a","b"].map(x => x.toUpperCase())`   | `["A","B"]`         |
| `filter()`        | Returns elements that satisfy a condition.                                | `[1,2,3,4].filter(x => x % 2 === 0)`    | `[2,4]`             |
| `reduce()`        | Combines all elements into a single value.                                | `[1,2,3].reduce((sum,x) => sum + x, 0)` | `6`                 |
| `find()`          | Returns the first element that matches a condition.                       | `[2,4,6,7].find(x => x % 2 !== 0)`      | `7`                 |
| `findIndex()`     | Returns the index of the first matching element.                          | `[2,4,6,7].findIndex(x => x % 2 !== 0)` | `3`                 |
| `some()`          | Returns `true` if at least one element matches.                           | `[2,4,5].some(x => x % 2 !== 0)`        | `true`              |
| `every()`         | Returns `true` if all elements match a condition.                         | `[2,4,6].every(x => x % 2 === 0)`       | `true`              |
| `forEach()`       | Executes a function for each element. Does not return a new array.        | `[1,2].forEach(x => console.log(x))`    | Prints `1` `2`      |
| `sort()`          | Sorts the array (mutates the original).                                   | `[3,1,2].sort((a,b)=>a-b)`              | `[1,2,3]`           |
| `includes()`      | Checks whether an array contains a value.                                 | `["A","B"].includes("B")`               | `true`              |
| `slice()`         | Returns a shallow copy of a portion of an array.                          | `[1,2,3,4].slice(1,3)`                  | `[2,3]`             |
| `splice()`        | Adds/removes elements from an array (mutates the original).               | `let a=[1,2,3]; a.splice(1,1);`         | `a = [1,3]`         |
| `concat()`        | Merges two or more arrays into a new array.                               | `[1,2].concat([3,4])`                   | `[1,2,3,4]`         |
| `flat()`          | Flattens nested arrays by one level (or more if specified).               | `[[1,2],[3]].flat()`                    | `[1,2,3]`           |
| `flatMap()`       | Maps each element and then flattens the result by one level.              | `["ab","cd"].flatMap(x => x.split(""))` | `["a","b","c","d"]` |
| `push()`          | Adds element(s) to the end of an array (mutates the original).            | `let a=[1,2]; a.push(3);`               | `a = [1,2,3]`       |
| `pop()`           | Removes and returns the last element (mutates the original).              | `let a=[1,2,3]; a.pop();`               | `a = [1,2]`         |
| `shift()`         | Removes and returns the first element (mutates the original).             | `let a=[1,2,3]; a.shift();`             | `a = [2,3]`         |
| `unshift()`       | Adds element(s) to the beginning of an array (mutates the original).      | `let a=[2,3]; a.unshift(1);`            | `a = [1,2,3]`       |
| `reverse()`       | Reverses the array in place (mutates the original).                       | `let a=[1,2,3]; a.reverse();`           | `a = [3,2,1]`       |
| `join()`          | Joins all elements into a string.                                         | `["a","b","c"].join("-")`               | `"a-b-c"`           |
| `indexOf()`       | Returns the first index of a value, or `-1` if not found.                 | `["a","b","c"].indexOf("b")`            | `1`                 |
| `lastIndexOf()`   | Returns the last index of a value.                                        | `[1,2,1].lastIndexOf(1)`                | `2`                 |
| `fill()`          | Fills all or part of an array with a static value (mutates the original). | `new Array(3).fill(0)`                  | `[0,0,0]`           |
| `Array.from()`    | Creates a new array from an iterable or array-like object.                | `Array.from("ABC")`                     | `["A","B","C"]`     |
| `Array.isArray()` | Checks whether a value is an array.                                       | `Array.isArray([1,2])`                  | `true`              |

## Property Binding vs Signals
**SIGNALS**
- Signals is like using getters and setters based component modification
- It used to store data and track dependencies changes like pub / sub model
- signal data are called in HTML through interpolation which converts the data to string.
- Not suitable for routing eg: 'routes/123' is different from routes/123
**HTML**
```
<p>{{ count() }}</p>

<button (click)="count.update(v => v + 1)">
    Increment
</button>
<button (click)="count.set(0)">
    Reset
</button>
```
**TS**
```
import { signal } from '@angular/core';
count = signal(0);
```

**PROPERTY BINDING**
- property binding is used pass data from a 
1. component to DOM 
2. Component to Child (Property Binding)
3. Parent to Child (Event Binding)
- It will pass the data in the same data type, unlike signals, it does not perform any conversions.
**HTML**
```
<img [src]="imageUrl">
<button [disabled]="isDisabled">Save</button>
```
**TS**
```
imageUrl = 'assets/profile.png';
isDisabled = true;
```

- both signals and property binding are used to pass data from a component to DOM
**HTML**
```
<p>Stock: {{ stock() }}</p>

<button
    [disabled]="stock() === 0"
    (click)="decreaseStock()">
    Buy
</button>
```

**TS**
```
export class ProductComponent {

  stock = signal(8);

  decreaseStock() {
    this.stock.update(v => v - 1);
  }
}
```
**When to use What**
- use signals for state management like log - in, globally
- use property binding for component communication

---

## PIPES
- Transform data for display without changing the original data
```
// HTML
<p>{{ currentDate | date: 'yyyy-MM-dd' }}</p>
<p>{{ price | currency:'INR' }}</p>
<p>{{ name | uppercase }}</p>
<p>{{ message | lowercase }}</p>
<p>{{ 12345 | number }}</p>
<p>{{ 5.6789 | number:'1.2-2' }}</p>
<p>{{ message | slice:0:5 }}</p>

// TS
import { Component } from '@angular/core';

@Component({
  standalone: true,
  imports: [CommonModule], // Import CommonModule for pipes
  templateUrl: './app.component.html',
})
export class AppComponent {
  currentDate = new Date();
  price = 499.99;
  name = "dharshini";
  message = "Hello World";
}
```
**Types of Pipes**
1. Built-in Pipes
2. Custom Pipes

1. **Built-in Pipes:**
- uppercase
- lowercase
- TitleCase
- currency
- date
- percentage
- number
- slice
- json

2. **Custom Pipes:**
```
//HTML
<p>{{name | greet}} </p>


//TS
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'greet'
})
export class GreetPipe implements PipeTransform {

  transform(value: string): string {
    return `Hello ${value}`;
  }

}

//Output
Hello Name
```
---
## ForkJoin
- It is like async - await, waits until a particular rendering completes
---
## QueryParamsHandling
- queryParamsHandling is an Angular Router option that controls what happens to the existing query parameters in the URL when you navigate to another route.

- Normally, Angular replaces the existing query parameters with the new ones you provide. queryParamsHandling lets you preserve or merge them instead.

- In this project, searching resutls are retrieved via the url:
 ```
    product-list?search=apple
 ```
 On going back, angular by default removes the query parameter and returns to
 ```
  product-list
 ```
- To preserve the QueryParams, we use QueryParamasHandling and set it to 'preserve'.

