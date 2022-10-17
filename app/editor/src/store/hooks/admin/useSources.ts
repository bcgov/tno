import { IPaged, ISourceModel, useApiAdminSources } from 'hooks/api-editor';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';
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
  const dispatch = useApiDispatcher();
  const [state, store] = useAdminStore();

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
        store.storeSources(
          state.sources.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addSource: async (model: ISourceModel) => {
        const response = await dispatch<ISourceModel>('add-source', () => api.addSource(model));
        store.storeSources([...state.sources, response.data]);
        return response.data;
      },
      updateSource: async (model: ISourceModel) => {
        const response = await dispatch<ISourceModel>('update-source', () =>
          api.updateSource(model),
        );
        store.storeSources(
          state.sources.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      deleteSource: async (model: ISourceModel) => {
        const response = await dispatch<ISourceModel>('delete-source', () =>
          api.deleteSource(model),
        );
        store.storeSources(state.sources.filter((ds) => ds.id !== response.data.id));
        return response.data;
      },
    }),
    // The state.sources will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dispatch, store, api],
  );

  return [state, controller];
};
