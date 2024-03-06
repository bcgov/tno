import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IIngestModel, IPaged, useApiAdminIngests } from 'tno-core';

interface IIngestController {
  findAllIngests: () => Promise<IIngestModel[]>;
  findIngests: () => Promise<IPaged<IIngestModel>>;
  getIngest: (id: number) => Promise<IIngestModel>;
  addIngest: (model: IIngestModel) => Promise<IIngestModel>;
  updateIngest: (model: IIngestModel) => Promise<IIngestModel>;
  setIngestEnabledStatus: (ingestId: number, newStatus: boolean) => Promise<IIngestModel>;
  resetIngestFailures: (ingestId: number) => Promise<IIngestModel>;
  deleteIngest: (model: IIngestModel) => Promise<IIngestModel>;
  storeIngest: (ingests: IIngestModel[]) => void;
}

export const useIngests = (): [IAdminState, IIngestController] => {
  const api = useApiAdminIngests();
  const dispatch = useAjaxWrapper();
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
        store.storeIngests((ingests) =>
          ingests.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addIngest: async (model: IIngestModel) => {
        const response = await dispatch<IIngestModel>('add-ingest', () => api.addIngest(model));
        store.storeIngests((ingests) => [...ingests, response.data]);
        return response.data;
      },
      updateIngest: async (model: IIngestModel) => {
        const response = await dispatch<IIngestModel>('update-ingest', () =>
          api.updateIngest(model),
        );
        store.storeIngests((ingests) =>
          ingests.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      setIngestEnabledStatus: async (ingestId: number, newStatus: boolean) => {
        const response = await dispatch<IIngestModel>('update-ingest-enabled', () =>
          api.setIngestEnabledStatus(ingestId, newStatus),
        );
        store.storeIngests((ingests) =>
          ingests.map((ds) => {
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
        store.storeIngests((ingests) => ingests.filter((ds) => ds.id !== response.data.id));
        return response.data;
      },
      storeIngest: (ingests: IIngestModel[]) => {
        store.storeIngests(ingests);
      },
      resetIngestFailures: async (ingestId: number) => {
        const response = await dispatch<IIngestModel>('reset-ingest-failures', () =>
          api.resetIngestFailures(ingestId),
        );
        store.storeIngests((ingests) =>
          ingests.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
    }),
    [dispatch, store, api],
  );

  return [state, controller];
};
