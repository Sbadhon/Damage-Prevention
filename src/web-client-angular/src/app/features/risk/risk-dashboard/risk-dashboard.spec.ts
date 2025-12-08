import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA, provideZonelessChangeDetection } from '@angular/core';
import { MockStore, provideMockStore } from '@ngrx/store/testing';
import { take } from 'rxjs/operators';

import { RiskDashboard } from './risk-dashboard';
import * as RiskSelectors from '@app/core/state/risk/risk.selectors';
import { RiskAssessment, RiskLevel } from '@app/core/state/risk/risk.models';

describe('RiskDashboard', () => {
  let fixture: ComponentFixture<RiskDashboard>;
  let component: RiskDashboard;
  let store: MockStore;

  const risks: RiskAssessment[] = [
    {
      riskId: '1',
      ticketId: 'T1',
      level: RiskLevel.High,
      score: 0.9,
      address: '',
      assessedAt: '',
    } as RiskAssessment,
    {
      riskId: '2',
      ticketId: 'T2',
      level: RiskLevel.High,
      score: 0.8,
      address: '',
      assessedAt: '',
    } as RiskAssessment,
    {
      riskId: '3',
      ticketId: 'T3',
      level: RiskLevel.Critical,
      score: 1.0,
      address: '',
      assessedAt: '',
    } as RiskAssessment,
    {
      riskId: '4',
      ticketId: 'T4',
      level: RiskLevel.Medium,
      score: 0.5,
      address: '',
      assessedAt: '',
    } as RiskAssessment,
    {
      riskId: '5',
      ticketId: 'T5',
      level: RiskLevel.Low,
      score: 0.2,
      address: '',
      assessedAt: '',
    } as RiskAssessment,
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RiskDashboard],
      providers: [
        provideZonelessChangeDetection(),
        provideMockStore({
          selectors: [
            {
              selector: RiskSelectors.selectRiskState,
              value: {
                risks: { items: risks, total: risks.length },
                loading: false,
              },
            },
          ],
        }),
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    store = TestBed.inject(MockStore);
    fixture = TestBed.createComponent(RiskDashboard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('dashboardStats$ should compute counts for each risk level', (done) => {
    component.dashboardStats$.pipe(take(1)).subscribe((stats) => {
      const map = new Map(stats.map((s) => [s.title, s.value]));

      expect(map.get('High Risk')).toBe(2);
      expect(map.get('Critical Risk')).toBe(1);
      expect(map.get('Medium Risk')).toBe(1);
      expect(map.get('Low Risk')).toBe(1);

      done();
    });
  });

  it('trackByRiskId should return riskId', () => {
    const item = risks[0];
    expect(component.trackByRiskId(0, item as any)).toBe(item.riskId);
  });
});
