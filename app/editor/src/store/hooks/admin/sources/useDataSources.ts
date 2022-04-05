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

  const getSources = () => state.dataSources;

  const controller = React.useMemo(
    () => ({
      findAllDataSources: async () => {
        const result = await dispatch('find-all-data-sources', () => api.findAllDataSources());
        store.storeDataSources(result);
        return result;
      },
      findDataSources: async () => {
        const result = await dispatch('find-data-sources', () => api.findDataSources());
        return result;
      },
      getDataSource: async (id: number) => {
        const result = await dispatch('get-data-source', () => api.getDataSource(id));
        store.storeDataSources(
          getSources().map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addDataSource: async (model: IDataSourceModel) => {
        const result = await dispatch('add-data-source', () => api.addDataSource(model));
        store.storeDataSources([...getSources(), result]);
        return result;
      },
      updateDataSource: async (model: IDataSourceModel) => {
        const result = await dispatch('update-data-source', () => api.updateDataSource(model));
        store.storeDataSources(
          getSources().map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteDataSource: async (model: IDataSourceModel) => {
        const result = await dispatch('delete-data-source', () => api.deleteDataSource(model));
        store.storeDataSources(getSources().filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [store],
  );

  return [state, controller];
};
