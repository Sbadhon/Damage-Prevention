import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TicketsDashboard } from './tickets-dashboard';

describe('TicketsDashboard', () => {
  let component: TicketsDashboard;
  let fixture: ComponentFixture<TicketsDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TicketsDashboard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TicketsDashboard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
