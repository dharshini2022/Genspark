import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { BusSearchResult } from '../../models/models';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-bus-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bus-details.component.html',
  styleUrl: './bus-details.component.scss'
})
export class BusDetailsComponent implements OnInit {
  bus: BusSearchResult | null = null;
  loading = false;

  constructor(private router: Router, private userSvc: UserService) {
    const nav = this.router.getCurrentNavigation();
    if (nav?.extras.state) {
      this.bus = nav.extras.state['bus'];
    }
  }

  ngOnInit(): void {
    if (!this.bus) {
      const saved = sessionStorage.getItem('selectedBus');
      if (saved) this.bus = JSON.parse(saved);
    }

    if (this.bus) {
      this.loading = true;
      this.userSvc.getScheduleDetails(this.bus.scheduleId).subscribe({
        next: (freshBus) => {
          this.bus = freshBus;
          sessionStorage.setItem('selectedBus', JSON.stringify(this.bus));
          this.loading = false;
        },
        error: () => this.loading = false
      });
    }

    if (this.bus && this.bus.features) {
      // Filter out any features that include the word 'contact' (case-insensitive)
      this.bus.features = this.bus.features.filter(f => !f.toLowerCase().includes('contact'));
    }
  }

  formatFeature(f: string): string {
    // Insert <br> before any emoji that is not at the very beginning
    return f.replace(/(?!^)([\u{1F300}-\u{1F9FF}\u{2600}-\u{26FF}\u{2700}-\u{27BF}])/gu, '<br>$1');
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleString('en-IN', {
      day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit'
    });
  }

  goBack(): void {
    this.router.navigate(['/user/home']);
  }

  proceed(): void {
    this.router.navigate(['/user/bus-layout'], { state: { bus: this.bus } });
  }
}

