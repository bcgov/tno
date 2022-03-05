import { ISortableModel } from '.';

export interface ILicenseModel extends ISortableModel<number> {
  description?: string;
  isEnabled: boolean;
  ttl: number;
}
