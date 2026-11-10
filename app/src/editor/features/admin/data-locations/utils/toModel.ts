import { type IDataLocationModel } from 'tno-core';

import { type IDataLocationForm } from '../interfaces';

export const toModel = (form: IDataLocationForm): IDataLocationModel => {
  return {
    ...form,
    connectionId: !form.connectionId ? undefined : +form.connectionId,
  };
};
