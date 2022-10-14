import { IIngestModel, IPaged, useApiAdminIngests } from 'hooks/api-editor';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface IIngestController {
  findAllIngests: () => Promise<IIngestModel[]>;
  findIngests: () => Promise<IPaged<IIngestModel>>;
  getIngest: (id: number) => Promise<IIngestModel>;
  addIngest: (model: IIngestModel) => Promise<IIngestModel>;
  updateIngest: (model: IIngestModel) => Promise<IIngestModel>;
  deleteIngest: (model: IIngestModel) => Promise<IIngestModel>;
}

export const useIngests = (): [IAdminState, IIngestController] => {
  const api = useApiAdminIngests();
  const dispatch = useApiDispatcher();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllIngests: async () => {
        const response = await dispatch<IIngestModel[]>('find-all-ingests', () =>
          api.findAllIngests(),
        );
        store.storeIngests(response.data);
        return response.data;
      },
      findIngests: async () => {
        const response = await dispatch<IPaged<IIngestModel>>('find-ingests', () =>
          api.findIngests(),
        );
        return response.data;
      },
      getIngest: async (id: number) => {
        const response = await dispatch<IIngestModel>('get-ingest', () => api.getIngest(id));
        store.storeIngests(
          state.ingests.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addIngest: async (model: IIngestModel) => {
        const response = await dispatch<IIngestModel>('add-ingest', () => api.addIngest(model));
        store.storeIngests([...state.ingests, response.data]);
        return response.data;
      },
      updateIngest: async (model: IIngestModel) => {
        const response = await dispatch<IIngestModel>('update-ingest', () =>
          api.updateIngest(model),
        );
        store.storeIngests(
          state.ingests.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      deleteIngest: async (model: IIngestModel) => {
        const response = await dispatch<IIngestModel>('delete-ingest', () =>
          api.deleteIngest(model),
        );
        store.storeIngests(state.ingests.filter((ds) => ds.id !== response.data.id));
        return response.data;
      },
    }),
    // The state.ingests will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dispatch, store, api],
  );

  return [state, controller];
};
