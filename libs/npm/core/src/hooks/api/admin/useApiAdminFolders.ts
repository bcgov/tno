import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IFolderContentModel, IFolderModel, useApi } from '..';

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
      return api.get<never, AxiosResponse<IFolderModel[]>, any>(`/admin/folders`);
    },
    getContentInFolder: (id: number, includeMaxTopicScore: boolean = false) => {
      return api.get<never, AxiosResponse<IFolderContentModel[]>, any>(
        `/admin/folders/${id}/content?includeMaxTopicScore=${includeMaxTopicScore}`,
      );
    },
    getFolder: (id: number, includeContent: boolean = false) => {
      return api.get<never, AxiosResponse<IFolderModel>, any>(
        `/admin/folders/${id}?includeContent=${includeContent}`,
      );
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
