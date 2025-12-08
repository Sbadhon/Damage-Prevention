import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ThemeService } from '@app/core/service/theme.service';
import { FormsModule } from '@angular/forms';

interface NavItem {
  route: string;
  label: string;
}

@Component({
  selector: 'dp-sidebar',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Sidebar {
  readonly themeService = inject(ThemeService);
  readonly navItems: NavItem[] = [
    { route: '/tickets', label: 'Tickets' },
    { route: '/workorders', label: 'Work Orders' },
    { route: '/risk', label: 'Risk Assessments' },
  ];
}
