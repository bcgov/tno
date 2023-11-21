import { ISourceModel } from 'tno-core';

import { ISourceForm } from '../interfaces';

export const toForm = (model: ISourceModel): ISourceForm => {
  return {
    ...model,
    mediaTypeId: model.mediaTypeId === undefined ? '' : model.mediaTypeId,
    mediaTypeSearchGroupId:
      model.mediaTypeSearchGroupId === undefined ? '' : model.mediaTypeSearchGroupId,
    ownerId: model.ownerId === undefined ? '' : model.ownerId,
    configuration: {
      ...model.configuration,
      timeZone: model.configuration.timeZone === undefined ? '' : model.configuration.timeZone,
    },
  };
};
