import { ISourceModel } from 'tno-core';

import { ISourceForm } from '../interfaces';

export const toModel = (form: ISourceForm): ISourceModel => {
  return {
    ...form,
    mediaTypeId: form.mediaTypeId === '' ? undefined : form.mediaTypeId,
    ownerId: form.ownerId === '' ? undefined : form.ownerId,
    configuration: {
      ...form.configuration,
      timeZone: form.configuration.timeZone === '' ? undefined : form.configuration.timeZone,
    },
  };
};
