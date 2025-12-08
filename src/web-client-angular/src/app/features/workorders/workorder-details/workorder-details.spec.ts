import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkorderDetails } from './workorder-details';

describe('WorkorderDetails', () => {
  let component: WorkorderDetails;
  let fixture: ComponentFixture<WorkorderDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WorkorderDetails]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WorkorderDetails);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
