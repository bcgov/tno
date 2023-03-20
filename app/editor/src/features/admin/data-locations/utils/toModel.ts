import { IDataLocationModel } from 'tno-core';

import { IDataLocationForm } from '../interfaces';

export const toModel = (form: IDataLocationForm): IDataLocationModel => {
  return {
    ...form,
    connectionId: !form.connectionId ? undefined : +form.connectionId,
  };
};
