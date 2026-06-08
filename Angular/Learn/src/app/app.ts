import { Component, signal } from '@angular/core';
import { Customer } from './customer/customer';
import { Product } from './product/product';
import { Login } from './login/login';
import { Register } from './register/register';
import { Account } from './account/account';
import { usernameSubject } from './rxjs/auth.operator';
import { Products } from './products/products';

@Component({
  selector: 'app-root',
  imports: [Customer, Products,Login,Register,Account],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  username = signal('Guest');
  constructor(){
    usernameSubject.subscribe({
      next: (un) => {
        this.username.set(un);
      }
    })
  }

  OnDestroy(){
    usernameSubject.unsubscribe();
  }

  protected readonly title = signal('Learn');
}
