import { Component, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { logout, usernameSubject, changeUsername } from '../../rxjs/auth.operator';

@Component({
  selector: 'app-header',
  imports: [RouterLink],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header implements OnInit {
  username = signal('Guest');

  constructor(private router: Router) {
    usernameSubject.subscribe({
      next: (un: string | undefined) => {
        this.username.set(un ? un : 'Guest');
      }
    });
  }

  ngOnInit() {
    changeUsername();
  }

  logout() {
    logout();
    this.router.navigate(['/']);
  }
}
