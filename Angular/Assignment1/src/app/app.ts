import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { usernameSubject } from './rxjs/auth.operator';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('Assignment1');
  isLoggedIn = signal(false);
  username = signal('Guest');
  constructor(){
    usernameSubject.subscribe({
      next: (un: string | undefined) => {
        this.username.set(un ? un:'Guest');
      }
    })
  }

  
}
