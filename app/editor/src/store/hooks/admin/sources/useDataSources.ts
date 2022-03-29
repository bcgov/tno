import { IDataSourceModel, IPaged, useApiAdminDataSources } from 'hooks/api-editor';
import React from 'react';
import { IAdminState, useAdminStore } from 'store/slices';

interface IDataSourceController {
  findDataSources: () => Promise<IPaged<IDataSourceModel>>;
  getDataSource: (id: number) => Promise<IDataSourceModel>;
  addDataSource: (model: IDataSourceModel) => Promise<IDataSourceModel>;
  updateDataSource: (model: IDataSourceModel) => Promise<IDataSourceModel>;
  deleteDataSource: (model: IDataSourceModel) => Promise<IDataSourceModel>;
}

export const useDataSources = (): [IAdminState, IDataSourceController] => {
  const api = useApiAdminDataSources();
  const [state, store] = useAdminStore();

  const getSources = () => state.dataSources;

  const controller = React.useMemo(
    () => ({
      findDataSources: async () => {
        const result = await api.findDataSources();
        store.storeDataSources(result.items);
        return result;
      },
      getDataSource: async (id: number) => {
        const result = await api.getDataSource(id);
        store.storeDataSources(
          getSources().map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addDataSource: async (model: IDataSourceModel) => {
        const result = await api.addDataSource(model);
        store.storeDataSources([...getSources(), result]);
        return result;
      },
      updateDataSource: async (model: IDataSourceModel) => {
        const result = await api.updateDataSource(model);
        store.storeDataSources(
          getSources().map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteDataSource: async (model: IDataSourceModel) => {
        const result = await api.deleteDataSource(model);
        store.storeDataSources(getSources().filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [store],
  );

  return [state, controller];
};
