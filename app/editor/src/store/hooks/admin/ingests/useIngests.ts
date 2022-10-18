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
        const result = await dispatch<IIngestModel[]>('find-all-ingests', () =>
          api.findAllIngests(),
        );
        store.storeIngests(result);
        return result;
      },
      findIngests: async () => {
        const result = await dispatch<IPaged<IIngestModel>>('find-ingests', () =>
          api.findIngests(),
        );
        return result;
      },
      getIngest: async (id: number) => {
        const result = await dispatch<IIngestModel>('get-ingest', () => api.getIngest(id));
        store.storeIngests(
          state.ingests.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addIngest: async (model: IIngestModel) => {
        const result = await dispatch<IIngestModel>('add-ingest', () => api.addIngest(model));
        store.storeIngests([...state.ingests, result]);
        return result;
      },
      updateIngest: async (model: IIngestModel) => {
        const result = await dispatch<IIngestModel>('update-ingest', () => api.updateIngest(model));
        store.storeIngests(
          state.ingests.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteIngest: async (model: IIngestModel) => {
        const result = await dispatch<IIngestModel>('delete-ingest', () => api.deleteIngest(model));
        store.storeIngests(state.ingests.filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    // The state.ingests will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dispatch, store, api],
  );

  return [state, controller];
};
