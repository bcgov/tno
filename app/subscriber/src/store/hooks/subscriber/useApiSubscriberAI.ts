import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, useApi } from 'tno-core';

export const useApiSubscriberAI = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    analyze: (prompt: string) => {
      return api.post<string, AxiosResponse<string>, { prompt: string }>(
        `/subscriber/ai/analyze`,
        { prompt },
        { headers: { 'Content-Type': 'application/json' } },
      );
    },
  }).current;
};
