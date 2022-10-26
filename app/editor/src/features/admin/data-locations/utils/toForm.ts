import { IDataLocationModel } from 'hooks';

import { IDataLocationForm } from '../interfaces';

export const toForm = (model: IDataLocationModel): IDataLocationForm => {
  return {
    ...model,
    connectionId: !model.connectionId ? '' : model.connectionId,
  };
};
