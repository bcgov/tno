import { type ISourceModel } from 'tno-core';

import { type ISourceForm } from '../interfaces';

export const toForm = (model: ISourceModel): ISourceForm => {
  return {
    ...model,
    mediaTypeId: model.mediaTypeId === undefined ? '' : model.mediaTypeId,
    mediaTypeSearchMappings:
      model.mediaTypeSearchMappings === undefined ? [] : model.mediaTypeSearchMappings,
    ownerId: model.ownerId === undefined ? '' : model.ownerId,
    configuration: {
      ...model.configuration,
      timeZone: model.configuration.timeZone === undefined ? '' : model.configuration.timeZone,
    },
  };
};
