import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface NavItem {
  route: string;
  label: string;
}

@Component({
  selector: 'dp-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Sidebar {
  readonly navItems: NavItem[] = [
    { route: '/tickets', label: 'Tickets' },
    { route: '/workorders', label: 'Work Orders' },
    { route: '/risk', label: 'Risk Assessments' },
  ];
}
