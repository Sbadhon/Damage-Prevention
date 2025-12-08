import {
  ChangeDetectionStrategy,
  Component,
  Input,
} from '@angular/core';
import { CommonModule, NgClass, NgFor } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

export interface DashboardStat {
  title: string;
  value: number;
  color: string; // Tailwind utility classes
}

@Component({
  standalone: true,
  selector: 'dp-dashboard-summary',
  imports: [CommonModule, MatCardModule, NgFor, NgClass],
  templateUrl: './dashboard-summary.html',
  styleUrls: ['./dashboard-summary.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardSummary {
  @Input({ required: true }) stats: DashboardStat[] = [];
}
