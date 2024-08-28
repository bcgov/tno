import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts, ITonePoolModel, useApi } from '../..';

export const useApiSubscriberTonePools = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getTonePools: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      return api.get<ITonePoolModel[], AxiosResponse<ITonePoolModel[]>, any>(
        `/subscriber/tonePools`,
        config,
      );
    },
    addMyTonePool: (tonePool: ITonePoolModel) => {
      return api.post<ITonePoolModel, AxiosResponse<ITonePoolModel>, any>(
        '/subscriber/tonePool',
        tonePool,
      );
    },
    getMyTonePool: (userId: number) => {
      console.log('getMyTonePool');
      return api.get<ITonePoolModel, AxiosResponse<ITonePoolModel>, any>(
        `/subscriber/tonePool/user/${userId}`,
      );
    },
  }).current;
};
