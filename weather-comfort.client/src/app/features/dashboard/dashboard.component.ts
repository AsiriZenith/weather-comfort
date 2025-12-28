import { CommonModule } from "@angular/common";
import { Component, OnInit, inject } from "@angular/core";
import { WeatherForecastService } from "../../shared/services";
import { DashboardCity } from "../../shared/models";

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  standalone: true,
  imports: [CommonModule],
})
export class DashboardComponent implements OnInit {
  public cities: DashboardCity[] = [];
  public isLoading = true;
  public error: string | null = null;
  
  #weatherForecastService = inject(WeatherForecastService);

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.isLoading = true;
    this.error = null;
    
    this.#weatherForecastService.getDashboardCities().subscribe({
      next: (result) => {
        this.cities = result;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading dashboard data:', error);
        this.error = 'Failed to load dashboard data. Please try again later.';
        this.isLoading = false;
      }
    });
  }
}
