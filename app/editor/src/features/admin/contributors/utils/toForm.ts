import { IContributorModel } from 'tno-core';

import { IContributorForm } from '../interfaces';

export const toForm = (model: IContributorModel): IContributorForm => {
  return {
    ...model,
    sourceId: model.sourceId !== undefined ? model.sourceId : '',
  };
};
