import { type IContributorModel } from 'tno-core';

import { type IContributorForm } from '../interfaces';

export const toModel = (values: IContributorForm): IContributorModel => {
  return {
    ...values,
    name: values.name.trim(),
    sourceId: values.sourceId !== '' ? values.sourceId : undefined,
  };
};
