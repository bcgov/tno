import { IDataSourceModel, IPaged, useApiAdminDataSources } from 'hooks/api-editor';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface IDataSourceController {
  findAllDataSources: () => Promise<IDataSourceModel[]>;
  findDataSources: () => Promise<IPaged<IDataSourceModel>>;
  getDataSource: (id: number) => Promise<IDataSourceModel>;
  addDataSource: (model: IDataSourceModel) => Promise<IDataSourceModel>;
  updateDataSource: (model: IDataSourceModel) => Promise<IDataSourceModel>;
  deleteDataSource: (model: IDataSourceModel) => Promise<IDataSourceModel>;
}

export const useDataSources = (): [IAdminState, IDataSourceController] => {
  const api = useApiAdminDataSources();
  const dispatch = useApiDispatcher();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllDataSources: async () => {
        const result = await dispatch<IDataSourceModel[]>('find-all-data-sources', () =>
          api.findAllDataSources(),
        );
        store.storeDataSources(result);
        return result;
      },
      findDataSources: async () => {
        const result = await dispatch<IPaged<IDataSourceModel>>('find-data-sources', () =>
          api.findDataSources(),
        );
        return result;
      },
      getDataSource: async (id: number) => {
        const result = await dispatch<IDataSourceModel>('get-data-source', () =>
          api.getDataSource(id),
        );
        store.storeDataSources(
          state.dataSources.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addDataSource: async (model: IDataSourceModel) => {
        const result = await dispatch<IDataSourceModel>('add-data-source', () =>
          api.addDataSource(model),
        );
        store.storeDataSources([...state.dataSources, result]);
        return result;
      },
      updateDataSource: async (model: IDataSourceModel) => {
        const result = await dispatch<IDataSourceModel>('update-data-source', () =>
          api.updateDataSource(model),
        );
        store.storeDataSources(
          state.dataSources.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteDataSource: async (model: IDataSourceModel) => {
        const result = await dispatch<IDataSourceModel>('delete-data-source', () =>
          api.deleteDataSource(model),
        );
        store.storeDataSources(state.dataSources.filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    // The state.dataSources will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dispatch, store, api],
  );

  return [state, controller];
};
