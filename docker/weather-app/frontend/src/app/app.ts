import { Component, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface DailyForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
  icon: string;
}

interface WeatherDetails {
  city: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
  icon: string;
  humidity: number;
  windSpeed: number;
  forecast: DailyForecast[];
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  searchQuery = signal('');
  weatherData = signal<WeatherDetails | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);
  tempUnit = signal<'C' | 'F'>('C');

  ngOnInit() {
    this.fetchWeather('London');
  }

  toggleUnit() {
    this.tempUnit.update(unit => unit === 'C' ? 'F' : 'C');
  }

  onSearch() {
    const query = this.searchQuery().trim();
    if (query) {
      this.fetchWeather(query);
    }
  }

  async fetchWeather(city: string) {
    this.loading.set(true);
    this.error.set(null);

    try {
      const apiHost = window.location.port === '4200' ? 'http://localhost:5000' : '';
      const response = await fetch(`${apiHost}/api/weather?city=${encodeURIComponent(city)}`);
      
      if (!response.ok) {
        const errData = await response.json().catch(() => ({}));
        throw new Error(errData.error || `Could not find weather details for "${city}".`);
      }

      const data: WeatherDetails = await response.json();
      this.weatherData.set(data);
      this.searchQuery.set(''); 
    } catch (err: any) {
      this.error.set(err.message || 'An unexpected error occurred.');
    } finally {
      this.loading.set(false);
    }
  }

  getFriendlyDay(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-US', { weekday: 'short' });
  }

  getFriendlyDate(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }
}
