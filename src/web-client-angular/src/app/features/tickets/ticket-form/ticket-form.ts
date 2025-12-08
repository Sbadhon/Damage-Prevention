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
import {
  MatProgressSpinnerModule,
} from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { Ticket } from '@app/core/state/ticket/ticket.models';

type NewTicketDto = Omit<Ticket, 'ticketId' | 'status' | 'createdAt'>;

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
  ],
  templateUrl: './ticket-form.html',
  styleUrls: ['./ticket-form.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TicketForm implements AfterViewInit, OnDestroy {
  private readonly dialogRef =
    inject<MatDialogRef<TicketForm, NewTicketDto | undefined>>(MatDialogRef);

  description = '';
  workType = '';
  address = '';

  readonly loadingCoords = signal(false);
  readonly coords = signal<{ lat: number; lon: number } | null>(null);
  readonly isSubmitting = signal(false);

  private map: L.Map | null = null;
  private marker: L.Marker | null = null;
  private destroyed = false;

  @ViewChild('mapContainer', { static: true })
  mapContainer!: ElementRef<HTMLDivElement>;

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
      // You can replace with a snackbar if you want.
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

      this.dialogRef.close(payload);
    } finally {
      this.isSubmitting.set(false);
    }
  }

  close(): void {
    this.dialogRef.close(undefined);
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
    this.initMap();
  }

  private initMap(): void {
    if (this.map || !this.mapContainer?.nativeElement) {
      return;
    }

    // Default center (can be anywhere; Minneapolis-ish)
    this.map = L.map(this.mapContainer.nativeElement).setView(
      [44.9778, -93.265],
      12,
    );

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
    }).addTo(this.map);
  }

  private updateMap(): void {
    if (!this.map) {
      this.initMap();
    }
    if (!this.map || !this.coords()) {
      return;
    }

    const { lat, lon } = this.coords()!;
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
    this.destroyed = true;
    if (this.map) {
      this.map.remove();
      this.map = null;
    }
  }
}
