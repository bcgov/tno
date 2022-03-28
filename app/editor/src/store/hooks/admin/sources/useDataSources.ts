import { IDataSourceModel, IPaged, useApiAdminDataSources } from 'hooks/api-editor';
import React from 'react';

interface IDataSourceController {
  findDataSources: () => Promise<IPaged<IDataSourceModel>>;
  getDataSource: (id: number) => Promise<IDataSourceModel>;
  addDataSource: (model: IDataSourceModel) => Promise<IDataSourceModel>;
  updateDataSource: (model: IDataSourceModel) => Promise<IDataSourceModel>;
  deleteDataSource: (model: IDataSourceModel) => Promise<IDataSourceModel>;
}

export const useDataSources = (): [IDataSourceController] => {
  const api = useApiAdminDataSources();

  const controller = React.useRef({
    findDataSources: async () => {
      const result = await api.findDataSources();
      return result;
    },
    getDataSource: async (id: number) => {
      const result = await api.getDataSource(id);
      return result;
    },
    addDataSource: async (model: IDataSourceModel) => {
      const result = await api.addDataSource(model);
      return result;
    },
    updateDataSource: async (model: IDataSourceModel) => {
      const result = await api.updateDataSource(model);
      return result;
    },
    deleteDataSource: async (model: IDataSourceModel) => {
      const result = await api.deleteDataSource(model);
      return result;
    },
  }).current;

  return [controller];
};
