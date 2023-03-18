import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IIngestScheduleModel, IScheduleModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiIngestSchedules = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    addSchedule: (model: IIngestScheduleModel) => {
      return api.post<IIngestScheduleModel, AxiosResponse<IScheduleModel>, any>(
        `/editor/ingests/schedules`,
        model,
      );
    },
  }).current;
};
