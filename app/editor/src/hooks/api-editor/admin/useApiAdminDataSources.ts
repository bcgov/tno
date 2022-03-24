import { defaultEnvelope, extractResponseData, ILifecycleToasts } from 'tno-core';

import { IDataSourceModel, IPaged, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
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

  return {
    findDataSources: () => {
      return extractResponseData<IPaged<IDataSourceModel>>(() => api.get(`/admin/data/sources`));
    },
    getDataSource: (id: number) => {
      return extractResponseData<IDataSourceModel>(() => api.get(`/admin/data/sources/${id}`));
    },
    addDataSource: (model: IDataSourceModel) => {
      return extractResponseData<IDataSourceModel>(() => api.post(`/admin/data/sources`, model));
    },
    updateDataSource: (model: IDataSourceModel) => {
      return extractResponseData<IDataSourceModel>(() =>
        api.put(`/admin/data/sources/${model.id}`, model),
      );
    },
    deleteDataSource: (model: IDataSourceModel) => {
      return extractResponseData<IDataSourceModel>(() =>
        api.delete(`/admin/data/sources/${model.id}`, { data: model }),
      );
    },
  };
};
