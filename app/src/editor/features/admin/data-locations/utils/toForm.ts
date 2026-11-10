import { type IDataLocationModel } from '@/shared';

import { type IDataLocationForm } from '../interfaces';

export const toForm = (model: IDataLocationModel): IDataLocationForm => {
  return {
    ...model,
    connectionId: !model.connectionId ? '' : model.connectionId,
  };
};
