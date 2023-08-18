import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IAVOverviewInstanceModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiEditorAVOverviews = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAVOverview: (publishedOn: Date | string) => {
      const params = {
        publishedOn,
      };
      return api.get<never, AxiosResponse<IAVOverviewInstanceModel | undefined>, any>(
        `/editor/reports/av/overviews?${toQueryString(params)}`,
      );
    },
    getAVOverview: (id: number) => {
      return api.get<never, AxiosResponse<IAVOverviewInstanceModel | undefined>, any>(
        `/editor/reports/av/overviews/${id}`,
      );
    },
    addAVOverview: (instance: IAVOverviewInstanceModel) => {
      return api.post<IAVOverviewInstanceModel, AxiosResponse<IAVOverviewInstanceModel>, any>(
        '/editor/reports/av/overviews',
        instance,
      );
    },
    updateAVOverview: (instance: IAVOverviewInstanceModel) => {
      return api.put<IAVOverviewInstanceModel, AxiosResponse<IAVOverviewInstanceModel>, any>(
        `/editor/reports/av/overviews/${instance.id}`,
        instance,
      );
    },
    deleteAVOverview: (instance: IAVOverviewInstanceModel) => {
      return api.delete<IAVOverviewInstanceModel, AxiosResponse<IAVOverviewInstanceModel>, any>(
        `/editor/reports/av/overviews/${instance.id}`,
        { data: instance },
      );
    },
  }).current;
};
