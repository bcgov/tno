export interface ILicenseModel {
  id: number;
  name: string;
  description?: string;
  isEnabled: boolean;
  sortOrder: number;
  ttl: number;
}
