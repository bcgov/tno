import { IPaged, ISourceModel, useApiAdminSources } from 'hooks/api-editor';
import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface ISourceController {
  findAllSources: () => Promise<ISourceModel[]>;
  findSources: () => Promise<IPaged<ISourceModel>>;
  getSource: (id: number) => Promise<ISourceModel>;
  addSource: (model: ISourceModel) => Promise<ISourceModel>;
  updateSource: (model: ISourceModel) => Promise<ISourceModel>;
  deleteSource: (model: ISourceModel) => Promise<ISourceModel>;
}

export const useSources = (): [IAdminState, ISourceController] => {
  const api = useApiAdminSources();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllSources: async () => {
        const response = await dispatch<ISourceModel[]>('find-all-sources', () =>
          api.findAllSources(),
        );
        store.storeSources(response.data);
        return response.data;
      },
      findSources: async () => {
        const response = await dispatch<IPaged<ISourceModel>>('find-sources', () =>
          api.findSources(),
        );
        return response.data;
      },
      getSource: async (id: number) => {
        const response = await dispatch<ISourceModel>('get-source', () => api.getSource(id));
        store.storeSources((sources) =>
          sources.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addSource: async (model: ISourceModel) => {
        const response = await dispatch<ISourceModel>('add-source', () => api.addSource(model));
        store.storeSources((sources) => [...sources, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateSource: async (model: ISourceModel) => {
        const response = await dispatch<ISourceModel>('update-source', () =>
          api.updateSource(model),
        );
        store.storeSources((sources) =>
          sources.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteSource: async (model: ISourceModel) => {
        const response = await dispatch<ISourceModel>('delete-source', () =>
          api.deleteSource(model),
        );
        store.storeSources((sources) => sources.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [dispatch, store, api, lookup],
  );

  return [state, controller];
};
