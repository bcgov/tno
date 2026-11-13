import { ITransferFilter } from './ITransferFilter';
import { ITransferFolder } from './ITransferFolder';
import { ITransferNotification } from './ITransferNotification';
import { ITransferProduct } from './ITransferProduct';
import { ITransferReport } from './ITransferReport';

export interface ITransferAccount {
  /** The user to transfer/copy objects from. */
  fromAccountId: number;
  /** The user to transfer/copy objects to. */
  toAccountId: number;
  /** Whether to change ownership of objects. */
  transferOwnership: boolean;
  /** Whether to also change the ownership of historical reports. */
  includeHistory: boolean;
  /** Whether the transfer account is ready to be submitted. */
  isReady: boolean;
  /** An array of notifications. */
  notifications: ITransferNotification[];
  /** An array of filters. */
  filters: ITransferFilter[];
  /** An array of folders. */
  folders: ITransferFolder[];
  /** An array of reports. */
  reports: ITransferReport[];
  /** An array of products. */
  products: ITransferProduct[];
}
