import { IContributorModel } from 'tno-core';

import { IContributorForm } from '../interfaces';

export const toModel = (values: IContributorForm): IContributorModel => {
  return {
    ...values,
    name: values.name.trim(),
    sourceId: values.sourceId !== '' ? values.sourceId : undefined,
  };
};
