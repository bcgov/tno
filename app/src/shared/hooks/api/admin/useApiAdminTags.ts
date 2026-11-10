import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IPaged, ITagModel, useApi } from '..';
import { ITagFilter } from '../interfaces/ITagFilter';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminTags = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllTags: () => {
      return api.get<never, AxiosResponse<ITagModel[]>, any>(`/admin/tags/all`);
    },
    findTags: (filter: ITagFilter) => {
      return api.get<never, AxiosResponse<IPaged<ITagModel>>, any>(
        `/admin/tags?${toQueryString(filter)}`,
      );
    },
    getTag: (id: number) => {
      return api.get<never, AxiosResponse<ITagModel>, any>(`/admin/tags/${id}`);
    },
    addTag: (model: ITagModel) => {
      return api.post<ITagModel, AxiosResponse<ITagModel>, any>(`/admin/tags`, model);
    },
    updateTag: (model: ITagModel) => {
      return api.put<ITagModel, AxiosResponse<ITagModel>, any>(`/admin/tags/${model.id}`, model);
    },
    deleteTag: (model: ITagModel) => {
      return api.delete<ITagModel, AxiosResponse<ITagModel>, any>(`/admin/tags/${model.id}`, {
        data: model,
      });
    },
  }).current;
};
