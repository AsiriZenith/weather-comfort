import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { WeatherForecastService } from '../../shared/services';
import { DashboardCity } from '../../shared/models';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  standalone: true,
  imports: [CommonModule],
})
export class DashboardComponent implements OnInit {
  cities = signal<DashboardCity[]>([]);
  isLoading = signal(false);
  error = signal<string | null>(null);

  #weatherForecastService = inject(WeatherForecastService);

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.isLoading.set(true);
    this.error.set(null);

    this.#weatherForecastService.getDashboardCities().subscribe({
      next: (result) => {
        this.cities.set(result);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading dashboard data:', error);
        this.error.set('Failed to load dashboard data. Please try again later.');
        this.isLoading.set(false);
      },
    });
  }
}
