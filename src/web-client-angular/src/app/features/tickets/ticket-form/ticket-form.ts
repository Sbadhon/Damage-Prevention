import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  OnDestroy,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import * as L from 'leaflet';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { Store } from '@ngrx/store';
import { Ticket } from '@app/core/state/ticket/ticket.models';
import { createTicket } from '@app/core/state/ticket/ticket.actions';

type NewTicketDto = Omit<Ticket, 'ticketId' | 'status' | 'createdAt'>;
type IconDefaultProto = { _getIconUrl?: () => string };
delete (L.Icon.Default.prototype as IconDefaultProto)._getIconUrl;

L.Icon.Default.mergeOptions({
  iconRetinaUrl:
    'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

@Component({
  standalone: true,
  selector: 'dp-ticket-form',
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatIconModule,
  ],
  templateUrl: './ticket-form.html',
  styleUrls: ['./ticket-form.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TicketForm implements AfterViewInit, OnDestroy {
  private readonly dialogRef =
    inject<MatDialogRef<TicketForm, void>>(MatDialogRef);
  private readonly store = inject(Store);

  description = '';
  workType = '';
  address = '';

  readonly loadingCoords = signal(false);
  readonly coords = signal<{ lat: number; lon: number } | null>(null);
  readonly isSubmitting = signal(false);

  private map: L.Map | null = null;
  private marker: L.Marker | null = null;

  @ViewChild('mapContainer', { static: false })
  mapContainer?: ElementRef<HTMLDivElement>;

  async onAddressBlur(): Promise<void> {
    if (!this.address) {
      this.coords.set(null);
      this.resetMap();
      return;
    }
    await this.updateCoordsForAddress(this.address);
  }

  async submit(): Promise<void> {
    if (!this.description || !this.workType || !this.address) {
      alert('Please fill out Description, Work Type, and Address.');
      return;
    }

    this.isSubmitting.set(true);
    try {
      const coords = this.coords() ?? { lat: 0, lon: 0 };
      const payload: NewTicketDto = {
        description: this.description,
        workType: this.workType,
        address: this.address,
        lat: coords.lat,
        lon: coords.lon,
      };
      this.store.dispatch(createTicket({ ticket: payload }));
      this.dialogRef.close();
    } finally {
      this.isSubmitting.set(false);
    }
  }

  close(): void {
    this.dialogRef.close();
  }

  async updateCoordsForAddress(address: string): Promise<void> {
    this.loadingCoords.set(true);
    this.coords.set(null);

    try {
      const url = `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(
        address,
      )}&format=json&limit=1`;

      const response = await fetch(url, {
        headers: { 'User-Agent': 'damage-prevention-angular-client' },
      });

      const data = await response.json();
      if (Array.isArray(data) && data.length > 0) {
        const lat = parseFloat(data[0].lat);
        const lon = parseFloat(data[0].lon);
        this.coords.set({ lat, lon });
        this.updateMap();
      } else {
        this.resetMap();
      }
    } catch {
      this.resetMap();
    } finally {
      this.loadingCoords.set(false);
    }
  }

  ngAfterViewInit(): void {
    if (this.coords()) {
      this.updateMap();
    }
  }

  private ensureMap(): void {
    if (this.map || !this.mapContainer?.nativeElement) {
      return;
    }
    this.map = L.map(this.mapContainer.nativeElement).setView(
      [44.9778, -93.265],
      12,
    );

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
    }).addTo(this.map);
  }

  private updateMap(): void {
    const current = this.coords();
    if (!current) {
      this.resetMap();
      return;
    }

    this.ensureMap();
    if (!this.map) {
      return;
    }

    const { lat, lon } = current;

    if (!this.marker) {
      this.marker = L.marker([lat, lon]).addTo(this.map);
    } else {
      this.marker.setLatLng([lat, lon]);
    }

    this.map.setView([lat, lon], 15);
  }

  private resetMap(): void {
    if (this.marker) {
      this.marker.remove();
      this.marker = null;
    }
  }

  ngOnDestroy(): void {
    if (this.map) {
      this.map.remove();
      this.map = null;
    }
  }
}
