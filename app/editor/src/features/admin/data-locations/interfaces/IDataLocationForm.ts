import { IDataLocationModel } from 'tno-core';

export interface IDataLocationForm extends Omit<IDataLocationModel, 'connectionId'> {
  connectionId: string | number;
}
