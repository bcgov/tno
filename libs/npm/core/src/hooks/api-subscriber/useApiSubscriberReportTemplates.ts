import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts, IReportTemplateModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberReportTemplates = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getReportTemplates: () => {
      return api.get<never, AxiosResponse<IReportTemplateModel[]>, any>(
        `/subscriber/report/templates`,
      );
    },
    getReportTemplate: (id: number) => {
      return api.get<never, AxiosResponse<IReportTemplateModel | undefined>, any>(
        `/subscriber/report/templates/${id}`,
      );
    },
  }).current;
};
