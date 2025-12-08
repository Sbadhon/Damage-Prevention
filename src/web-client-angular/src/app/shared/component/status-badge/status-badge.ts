import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';

import { TicketStatus } from '@app/core/state/ticket/ticket.models';
import { WorkOrderStatus } from '@app/core/state/workorders/workorders.models';
import { RiskLevel } from '@app/core/state/risk/risk.models';

type StatusLevel = TicketStatus | WorkOrderStatus | RiskLevel;

@Component({
  standalone: true,
  selector: 'dp-status-badge',
  imports: [CommonModule, NgClass],
  template: `
    <span
      class="px-2.5 py-1 text-xs font-semibold rounded-full border"
      [ngClass]="statusClasses[level]"
    >
      {{ level }}
    </span>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusBadge {
  @Input({ required: true }) level!: StatusLevel;

  readonly defaultClass =
    'bg-gray-100 text-gray-800 border-gray-300';

  readonly statusClasses: Record<string, string> = {
    ['Open']: 'bg-blue-100 text-blue-800 border-blue-300',
    ['In Progress']: 'bg-yellow-100 text-yellow-800 border-yellow-300',
    ['Completed']: 'bg-green-100 text-green-800 border-green-300',
    ['Cancelled']: 'bg-red-100 text-red-800 border-red-300',
    ['Pending']: 'bg-yellow-100 text-yellow-800 border-yellow-300',
    ['Assigned']: 'bg-indigo-100 text-indigo-800 border-indigo-300',
    ['Low']: 'bg-green-100 text-green-800 border-green-300',
    ['Medium']: 'bg-yellow-100 text-yellow-800 border-yellow-300',
    ['High']: 'bg-orange-100 text-orange-800 border-orange-300',
    ['Critical']: 'bg-red-100 text-red-800 border-red-300',
  };
}
