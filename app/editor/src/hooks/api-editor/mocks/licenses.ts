import { ILicenseModel } from '..';

export const mockLicenses: ILicenseModel[] = [
  {
    id: 1,
    name: 'Regular Expire',
    description: '',
    sortOrder: 0,
    enabled: true,
    ttl: 90,
  },
  {
    id: 1,
    name: 'Special Expire',
    description: '',
    sortOrder: 0,
    enabled: true,
    ttl: 140,
  },
  {
    id: 1,
    name: 'Never Expire',
    description: '',
    sortOrder: 0,
    enabled: true,
    ttl: -1,
  },
];
