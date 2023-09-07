import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IReportResultModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiEditorReportInstances = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    previewReportInstance: (reportInstanceId: number) => {
      return api.post<never, AxiosResponse<IReportResultModel>, any>(
        `/editor/report/instances/${reportInstanceId}/preview`,
      );
    },
  }).current;
};
