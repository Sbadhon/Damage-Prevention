export enum RiskLevel {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical',
}

export interface RiskAssessment {
  riskId: string;
  ticketId: string;
  score: number;
  level: RiskLevel;
  workType: string;
  address: string;
  lat: number;
  lon: number;
  assessedAt: string;
}
