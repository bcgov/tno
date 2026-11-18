import { AISectionStatusName } from '../constants';

export interface IReportAISectionStatusModel {
  sectionId: number;
  name: string;
  label: string;
  status: AISectionStatusName;
  error?: string;
  expiresOn?: string;
}
