import { type IDataLocationModel } from '@/shared';

export interface IDataLocationForm extends Omit<IDataLocationModel, 'connectionId'> {
  connectionId: string | number;
}
