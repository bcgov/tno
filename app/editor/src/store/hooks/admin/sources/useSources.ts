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
        const result = await dispatch<ISourceModel[]>('find-all-sources', () =>
          api.findAllSources(),
        );
        store.storeSources(result);
        return result;
      },
      findSources: async () => {
        const result = await dispatch<IPaged<ISourceModel>>('find-sources', () =>
          api.findSources(),
        );
        return result;
      },
      getSource: async (id: number) => {
        const result = await dispatch<ISourceModel>('get-source', () => api.getSource(id));
        store.storeSources(
          state.sources.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addSource: async (model: ISourceModel) => {
        const result = await dispatch<ISourceModel>('add-source', () => api.addSource(model));
        store.storeSources([...state.sources, result]);
        return result;
      },
      updateSource: async (model: ISourceModel) => {
        const result = await dispatch<ISourceModel>('update-source', () => api.updateSource(model));
        store.storeSources(
          state.sources.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteSource: async (model: ISourceModel) => {
        const result = await dispatch<ISourceModel>('delete-source', () => api.deleteSource(model));
        store.storeSources(state.sources.filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    // The state.sources will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dispatch, store, api],
  );

  return [state, controller];
};
