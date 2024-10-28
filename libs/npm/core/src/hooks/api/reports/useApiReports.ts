import moment from 'moment';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { useDownload } from '../../useDownload';
import { useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiReports = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);
  const download = useDownload(api);

  return React.useRef({
    // Lookups
    generateCBRAReport: (from: Date, to?: Date | null) => {
      const params = {
        from: moment(from).format('YYYY-MM-DDT00:00:00'),
        to: to ? moment(to).format('YYYY-MM-DDT11:59:59') : undefined,
      };
      return download({
        url: `/reports/cbra?${toQueryString(params)}`,
        method: 'post',
        fileName: 'cbra.xlsx',
      });
    },
  }).current;
};
