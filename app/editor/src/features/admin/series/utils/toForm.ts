import { ISeriesModel } from 'hooks/api-editor';

import { ISeriesForm } from '../interfaces';

export const toForm = (model: ISeriesModel): ISeriesForm => {
  return {
    ...model,
    sourceId: model.sourceId !== undefined ? model.sourceId : '',
  };
};
