export interface IIngestMessageModel {
  id: number;
  name: string;
  isEnabled: boolean;
  retryLimit: number;
  failedAttempts: number;
  lastRanOn?: string;
}
