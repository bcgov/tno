import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IAlertModel, useApiAnonAlerts } from 'tno-core';

interface IAlertController {
  findAlert: () => Promise<IAlertModel>;
}

export const useAnonAlerts = (): [IAdminState, IAlertController] => {
  const api = useApiAnonAlerts();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAlert: async () => {
        const response = await dispatch<IAlertModel>('find-alert', () => api.findAlert());
        store.storeAlerts([response.data]);
        return response.data;
      },
    }),
    [api, dispatch, store],
  );

  return [state, controller];
};
