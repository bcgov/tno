import { ISourceModel } from 'tno-core';

import { ISourceForm } from '../interfaces';

export const toForm = (model: ISourceModel): ISourceForm => {
  return {
    ...model,
    productId: model.productId === undefined ? '' : model.productId,
    ownerId: model.ownerId === undefined ? '' : model.ownerId,
    configuration: {
      ...model.configuration,
      timeZone: model.configuration.timeZone === undefined ? '' : model.configuration.timeZone,
    },
  };
};
