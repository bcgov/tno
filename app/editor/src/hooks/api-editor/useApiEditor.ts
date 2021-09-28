import React from 'react';
import { CustomAxios, defaultEnvelope, LifecycleToasts } from 'utils';

import { Settings } from '.';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiEditor = (
  axiosOptions: {
    lifecycleToasts?: LifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  return React.useMemo(
    () =>
      CustomAxios({
        ...axiosOptions,
        baseURL: axiosOptions.baseURL ?? Settings.ApiPath,
      }),
    [axiosOptions],
  );
};

export default useApiEditor;
