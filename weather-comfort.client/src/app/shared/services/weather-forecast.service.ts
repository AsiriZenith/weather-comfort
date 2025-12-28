import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { DashboardCity } from '../models/dashboard-city';

@Injectable({
  providedIn: 'root',
})
export class WeatherForecastService {
  private apiUrl = 'https://localhost:7287/api/weather/dashboard';

  #http = inject(HttpClient);

  getDashboardCities(): Observable<DashboardCity[]> {
    return this.#http.get<DashboardCity[]>(this.apiUrl);
  }
}
