import moment from 'moment';
import React from 'react';
import {
  defaultEnvelope,
  extractResponseData,
  LifecycleToasts,
  toQueryString,
  useDownload,
} from 'tno-core';

import { useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiReports = (
  options: {
    lifecycleToasts?: LifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);
  const download = useDownload(api);

  return React.useMemo(
    () => ({
      // Lookups
      generateCBRAReport: (from: Date, to?: Date | null) => {
        const params = {
          from: moment(from).format('YYYY-MM-DDT00:00:00'),
          to: to ? moment(to).format('YYYY-MM-DDT:11:59:59') : undefined,
        };
        return extractResponseData<any>(() =>
          download({
            url: `/reports/cbra?${toQueryString(params)}`,
            method: 'post',
            fileName: 'cbra.xlsx',
          }),
        );
      },
    }),
    [download],
  );
};
