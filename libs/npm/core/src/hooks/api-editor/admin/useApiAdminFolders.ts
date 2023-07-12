import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IFolderModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminFolders = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllFolders: () => {
      return api.get<IFolderModel[], AxiosResponse<IFolderModel[]>, any>(`/admin/folders`);
    },
    getFolder: (id: number) => {
      return api.get<IFolderModel, AxiosResponse<IFolderModel>, any>(`/admin/folders/${id}`);
    },
    addFolder: (model: IFolderModel) => {
      return api.post<IFolderModel, AxiosResponse<IFolderModel>, any>(`/admin/folders`, model);
    },
    updateFolder: (model: IFolderModel) => {
      return api.put<IFolderModel, AxiosResponse<IFolderModel>, any>(
        `/admin/folders/${model.id}`,
        model,
      );
    },
    deleteFolder: (model: IFolderModel) => {
      return api.delete<IFolderModel, AxiosResponse<IFolderModel>, any>(
        `/admin/folders/${model.id}`,
        {
          data: model,
        },
      );
    },
  }).current;
};
