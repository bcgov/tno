import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IAlertModel, useApiAlerts } from 'tno-core';

interface IAlertController {
  findAlert: () => Promise<IAlertModel>;
}

export const useAlerts = (): [IAdminState, IAlertController] => {
  const api = useApiAlerts();
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
