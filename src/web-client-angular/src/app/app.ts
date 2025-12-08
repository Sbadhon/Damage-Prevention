import { Component, signal } from '@angular/core';
import { Shell } from "./core/layout/shell/shell";

@Component({
  standalone: true,
  selector: 'dp-root',
  imports: [Shell],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('web-client-angular');
}
