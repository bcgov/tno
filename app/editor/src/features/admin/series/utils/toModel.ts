import { ISeriesModel } from 'tno-core';

import { ISeriesForm } from '../interfaces';

export const toModel = (values: ISeriesForm): ISeriesModel => {
  return {
    ...values,
    name: values.name.trim(),
    sourceId: values.sourceId !== '' ? values.sourceId : undefined,
  };
};
