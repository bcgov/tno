import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IAlertModel, useApiAdminAlerts } from 'tno-core';

interface IAlertController {
  findAlert: () => Promise<IAlertModel>;
  addAlert: (model: IAlertModel) => Promise<IAlertModel>;
  updateAlert: (model: IAlertModel) => Promise<IAlertModel>;
  deleteAlert: (model: IAlertModel) => Promise<IAlertModel>;
}

export const useAlerts = (): [IAdminState, IAlertController] => {
  const api = useApiAdminAlerts();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAlert: async () => {
        const response = await dispatch<IAlertModel>('find-alert', () => api.findAlert());
        store.storeAlerts([response.data]);
        return response.data;
      },
      addAlert: async (model: IAlertModel) => {
        const response = await dispatch<IAlertModel>('add-alert', () => api.addAlert(model));
        store.storeAlerts((alerts) => [...alerts, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateAlert: async (model: IAlertModel) => {
        const response = await dispatch<IAlertModel>('update-alert', () => api.updateAlert(model));
        store.storeAlerts((alerts) =>
          alerts.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteAlert: async (model: IAlertModel) => {
        const response = await dispatch<IAlertModel>('delete-alert', () => api.deleteAlert(model));
        store.storeTags((tags) => tags.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [state, controller];
};
