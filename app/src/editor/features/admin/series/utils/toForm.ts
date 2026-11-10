import { type ISeriesModel } from '@/shared';

import { type ISeriesForm } from '../interfaces';

export const toForm = (model: ISeriesModel): ISeriesForm => {
  return {
    ...model,
    sourceId: model.sourceId !== undefined ? model.sourceId : '',
  };
};
