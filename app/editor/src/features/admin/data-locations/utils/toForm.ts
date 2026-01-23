import { type IDataLocationModel } from 'tno-core';

import { type IDataLocationForm } from '../interfaces';

export const toForm = (model: IDataLocationModel): IDataLocationForm => {
  return {
    ...model,
    connectionId: !model.connectionId ? '' : model.connectionId,
  };
};
