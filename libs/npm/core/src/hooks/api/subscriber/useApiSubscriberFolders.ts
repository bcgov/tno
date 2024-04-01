import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, IFolderModel, ILifecycleToasts, useApi } from '../..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberFolders = (
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
      return api.get<IFolderModel[], AxiosResponse<IFolderModel[]>, any>(`/subscriber/folders`);
    },
    findMyFolders: () => {
      return api.get<IFolderModel[], AxiosResponse<IFolderModel[]>, any>(
        `/subscriber/folders/my-folders`,
      );
    },
    getFolder: (id: number, includeContent: boolean = false) => {
      return api.get<IFolderModel, AxiosResponse<IFolderModel>, any>(`/subscriber/folders/${id}?includeContent=${includeContent}`);
    },
    addFolder: (model: IFolderModel) => {
      return api.post<IFolderModel, AxiosResponse<IFolderModel>, any>(`/subscriber/folders`, model);
    },
    updateFolder: (model: IFolderModel) => {
      return api.put<IFolderModel, AxiosResponse<IFolderModel>, any>(
        `/subscriber/folders/${model.id}`,
        model,
      );
    },
    deleteFolder: (model: IFolderModel) => {
      return api.delete<IFolderModel, AxiosResponse<IFolderModel>, any>(
        `/subscriber/folders/${model.id}`,
        {
          data: model,
        },
      );
    },
  }).current;
};
