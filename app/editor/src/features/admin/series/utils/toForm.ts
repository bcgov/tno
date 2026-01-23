import { type ISeriesModel } from 'tno-core';

import { type ISeriesForm } from '../interfaces';

export const toForm = (model: ISeriesModel): ISeriesForm => {
  return {
    ...model,
    sourceId: model.sourceId !== undefined ? model.sourceId : '',
  };
};
