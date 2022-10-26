import { IDataLocationModel } from 'hooks';

export interface IDataLocationForm extends Omit<IDataLocationModel, 'connectionId'> {
  connectionId: string | number;
}
