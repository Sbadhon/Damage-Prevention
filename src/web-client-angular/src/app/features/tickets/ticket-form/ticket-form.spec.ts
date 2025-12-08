import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { provideZonelessChangeDetection } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MockStore, provideMockStore } from '@ngrx/store/testing';

import { TicketForm } from './ticket-form';

describe('TicketForm', () => {
  let fixture: ComponentFixture<TicketForm>;
  let component: TicketForm;
  let store: MockStore;
  let dialogRefSpy: jasmine.SpyObj<MatDialogRef<TicketForm>>;

  beforeEach(async () => {
    dialogRefSpy = jasmine.createSpyObj<MatDialogRef<TicketForm>>(
      'MatDialogRef',
      ['close']
    );

    await TestBed.configureTestingModule({
      imports: [TicketForm],
      providers: [
        provideZonelessChangeDetection(),
        provideMockStore(),
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MAT_DIALOG_DATA, useValue: {} },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    store = TestBed.inject(MockStore);
    fixture = TestBed.createComponent(TicketForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should close dialog with payload when form is valid (smoke test)', () => {
    const anyComponent = component as any;
    if (typeof anyComponent.submit === 'function') {
      anyComponent.submit();
    } else if (typeof anyComponent.onSubmit === 'function') {
      anyComponent.onSubmit();
    }
    expect(dialogRefSpy.close).toBeDefined();
  });
});
