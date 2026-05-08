import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { AuthResponse } from '../../models/models';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent implements OnInit {
  user: AuthResponse | null = null;
  badgeClass = 'badge-primary';

  constructor(private auth: AuthService) {}

  ngOnInit(): void {
    this.auth.user$.subscribe(u => {
      this.user = u;
      const map: Record<string, string> = { Customer: 'badge-info', Operator: 'badge-warning', Admin: 'badge-danger' };
      this.badgeClass = map[u?.role ?? ''] ?? 'badge-primary';
    });
  }

  logout(): void { this.auth.logout(); }
}
