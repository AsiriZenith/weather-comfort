import { Component, OnInit, inject } from '@angular/core';
import { WeatherForecast } from './shared/models';
import { WeatherForecastService } from './shared/services';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  standalone: true
})
export class AppComponent implements OnInit {
  public forecasts: WeatherForecast[] = [];

  private weatherForecastService = inject(WeatherForecastService);

  ngOnInit() {
    this.getForecasts();
  }

  getForecasts() {
    this.weatherForecastService.getWeatherForecast().subscribe((result) => {
      this.forecasts = result;
    }, (error) => {
      console.error(error);
    });
  }

  title = 'weather-comfort.client';
}
