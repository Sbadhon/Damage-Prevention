import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkordersDashboard } from './workorders-dashboard';

describe('WorkordersDashboard', () => {
  let component: WorkordersDashboard;
  let fixture: ComponentFixture<WorkordersDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WorkordersDashboard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WorkordersDashboard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
