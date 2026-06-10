import { Component, signal } from '@angular/core';
import { Customer } from './customer/customer';
import { Product } from './product/product';
import { Login } from './login/login';
import { Register } from './register/register';
import { Account } from './account/account';
import { usernameSubject } from './rxjs/auth.operator';
import { Products } from './products/products';
import { RouterOutlet } from '@angular/router';
import { Navigation } from './navigation/navigation';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet,Navigation],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  username = signal('Guest');
  constructor(){
    usernameSubject.subscribe({
      next: (un) => {
        this.username.set(un ? un:'Guest');
      }
    })
  }

  OnDestroy(){
    usernameSubject.unsubscribe();
  }

  protected readonly title = signal('Learn');
}
