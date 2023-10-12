import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IPaged, ITopicModel, useApi } from '..';
import { ITopicFilter } from '../interfaces/ITopicFilter';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminTopics = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllTopics: () => {
      return api.get<never, AxiosResponse<ITopicModel[]>, any>(`/admin/topics/all`);
    },
    findTopics: (filter: ITopicFilter) => {
      return api.get<never, AxiosResponse<IPaged<ITopicModel>>, any>(
        `/admin/topics?${toQueryString(filter)}`,
      );
    },
    getTopic: (id: number) => {
      return api.get<never, AxiosResponse<ITopicModel>, any>(`/admin/topics/${id}`);
    },
    addTopic: (model: ITopicModel) => {
      return api.post<ITopicModel, AxiosResponse<ITopicModel>, any>(`/admin/topics`, model);
    },
    updateTopic: (model: ITopicModel) => {
      return api.put<ITopicModel, AxiosResponse<ITopicModel>, any>(
        `/admin/topics/${model.id}`,
        model,
      );
    },
    deleteTopic: (model: ITopicModel) => {
      return api.delete<ITopicModel, AxiosResponse<ITopicModel>, any>(`/admin/topics/${model.id}`, {
        data: model,
      });
    },
  }).current;
};
