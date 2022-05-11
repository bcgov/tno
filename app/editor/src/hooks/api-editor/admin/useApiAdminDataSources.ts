import React from 'react';
import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IDataSourceModel, IPaged, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminDataSources = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useMemo(
    () => ({
      findAllDataSources: () => {
        return api.get<IDataSourceModel[]>(`/admin/data/sources`);
      },
      findDataSources: () => {
        return api.get<IPaged<IDataSourceModel>>(`/admin/data/sources/find`);
      },
      getDataSource: (id: number) => {
        return api.get<IDataSourceModel>(`/admin/data/sources/${id}`);
      },
      addDataSource: (model: IDataSourceModel) => {
        return api.post<IDataSourceModel>(`/admin/data/sources`, model);
      },
      updateDataSource: (model: IDataSourceModel) => {
        return api.put<IDataSourceModel>(`/admin/data/sources/${model.id}`, model);
      },
      deleteDataSource: (model: IDataSourceModel) => {
        return api.delete<IDataSourceModel>(`/admin/data/sources/${model.id}`, { data: model });
      },
    }),
    [api],
  );
};
