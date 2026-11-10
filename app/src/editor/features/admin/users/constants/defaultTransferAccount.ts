import { type ITransferAccount } from '@/shared';

export const defaultTransferAccount: ITransferAccount = {
  fromAccountId: 0,
  toAccountId: 0,
  transferOwnership: true,
  includeHistory: true,
  isReady: false,
  notifications: [],
  filters: [],
  folders: [],
  reports: [],
  products: [],
};
