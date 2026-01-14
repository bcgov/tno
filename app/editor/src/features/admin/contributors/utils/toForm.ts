import { type IContributorModel } from 'tno-core';

import { type IContributorForm } from '../interfaces';

export const toForm = (model: IContributorModel): IContributorForm => {
  return {
    ...model,
    sourceId: model.sourceId !== undefined ? model.sourceId : '',
  };
};
