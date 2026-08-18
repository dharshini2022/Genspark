import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-access-denied',
  imports: [CommonModule],
  templateUrl: './access-denied.html',
  styleUrl: './access-denied.css'
})
export class AccessDenied implements OnInit {
  isLoggedIn = false;

  constructor(private router: Router) {}

  ngOnInit(): void {
    const cachedUser = sessionStorage.getItem('user');
    this.isLoggedIn = !!cachedUser;
  }

  handleAction(): void {
    if (this.isLoggedIn) {
      this.router.navigate(['/dashboard']);
    } else {
      this.router.navigate(['/login']);
    }
  }
}
