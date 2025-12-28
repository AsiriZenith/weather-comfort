import { Component, OnInit, inject } from '@angular/core';
import { DashboardComponent } from './features/dashboard/dashboard.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  standalone: true,
  imports: [DashboardComponent]
})
export class AppComponent {
  title = 'weather-comfort.client';
}
