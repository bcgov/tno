import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IContentListModel, IContentModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiMorningReports = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    updateContent: (action: IContentListModel) => {
      return api.put<IContentListModel, AxiosResponse<IContentModel[]>, any>(
        `/editor/morning/reports`,
        action,
      );
    },
  }).current;
};
