import moment from 'moment';
import {
  defaultEnvelope,
  extractResponseData,
  ILifecycleToasts,
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
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);
  const download = useDownload(api);

  return {
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
  };
};
