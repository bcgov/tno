import { IConnectionModel, useApiAdminConnections } from 'hooks/api-editor';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface IConnectionController {
  findAllConnections: () => Promise<IConnectionModel[]>;
  getConnection: (id: number) => Promise<IConnectionModel>;
  addConnection: (model: IConnectionModel) => Promise<IConnectionModel>;
  updateConnection: (model: IConnectionModel) => Promise<IConnectionModel>;
  deleteConnection: (model: IConnectionModel) => Promise<IConnectionModel>;
}

export const useConnections = (): [IAdminState, IConnectionController] => {
  const api = useApiAdminConnections();
  const dispatch = useApiDispatcher();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllConnections: async () => {
        const result = await dispatch<IConnectionModel[]>('find-all-connections', () =>
          api.findAllConnections(),
        );
        store.storeConnections(result);
        return result;
      },
      getConnection: async (id: number) => {
        const result = await dispatch<IConnectionModel>('get-connection', () =>
          api.getConnection(id),
        );
        store.storeConnections(
          state.connections.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addConnection: async (model: IConnectionModel) => {
        const result = await dispatch<IConnectionModel>('add-connection', () =>
          api.addConnection(model),
        );
        store.storeConnections([...state.connections, result]);
        return result;
      },
      updateConnection: async (model: IConnectionModel) => {
        const result = await dispatch<IConnectionModel>('update-connection', () =>
          api.updateConnection(model),
        );
        store.storeConnections(
          state.connections.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteConnection: async (model: IConnectionModel) => {
        const result = await dispatch<IConnectionModel>('delete-connection', () =>
          api.deleteConnection(model),
        );
        store.storeConnections(state.connections.filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    // The state.connections will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dispatch, store, api],
  );

  return [state, controller];
};
