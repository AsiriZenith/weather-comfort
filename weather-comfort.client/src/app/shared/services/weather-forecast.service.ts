import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { WeatherForecast } from '../models/weatherForecast';

@Injectable({
  providedIn: 'root',
})
export class WeatherForecastService {
  private apiUrl = 'https://localhost:7287/weatherforecast';

  #http = inject(HttpClient);

  getWeatherForecast(): Observable<WeatherForecast[]> {
    return this.#http.get<WeatherForecast[]>(this.apiUrl);
  }
}
