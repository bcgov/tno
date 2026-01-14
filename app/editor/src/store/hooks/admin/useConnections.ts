import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { type IAdminState, useAdminStore } from 'store/slices';
import { type IConnectionModel, useApiAdminConnections } from 'tno-core';

interface IConnectionController {
  findAllConnections: () => Promise<IConnectionModel[]>;
  getConnection: (id: number) => Promise<IConnectionModel>;
  addConnection: (model: IConnectionModel) => Promise<IConnectionModel>;
  updateConnection: (model: IConnectionModel) => Promise<IConnectionModel>;
  deleteConnection: (model: IConnectionModel) => Promise<IConnectionModel>;
}

export const useConnections = (): [IAdminState, IConnectionController] => {
  const api = useApiAdminConnections();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllConnections: async () => {
        const response = await dispatch<IConnectionModel[]>(
          'find-all-connections',
          async () => await api.findAllConnections(),
        );
        store.storeConnections(response.data);
        return response.data;
      },
      getConnection: async (id: number) => {
        const response = await dispatch<IConnectionModel>(
          'get-connection',
          async () => await api.getConnection(id),
        );
        store.storeConnections((connections) =>
          connections.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addConnection: async (model: IConnectionModel) => {
        const response = await dispatch<IConnectionModel>(
          'add-connection',
          async () => await api.addConnection(model),
        );
        store.storeConnections((connections) => [...connections, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateConnection: async (model: IConnectionModel) => {
        const response = await dispatch<IConnectionModel>(
          'update-connection',
          async () => await api.updateConnection(model),
        );
        store.storeConnections((connections) =>
          connections.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteConnection: async (model: IConnectionModel) => {
        const response = await dispatch<IConnectionModel>(
          'delete-connection',
          async () => await api.deleteConnection(model),
        );
        store.storeConnections((connections) =>
          connections.filter((ds) => ds.id !== response.data.id),
        );
        await lookup.getLookups();
        return response.data;
      },
    }),
    [dispatch, store, api, lookup],
  );

  return [state, controller];
};
