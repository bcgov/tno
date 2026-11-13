import { AxiosResponse } from 'axios';
import React from 'react';

import { type IFilterPageFilter } from '@/editor/features/admin/filters/interfaces';
import { toQueryString, useApi, type IFilterModel, type IPaged } from '@/shared';

/**
 * The admin filter list's paged search. The full list (useApiAdminFilters.findFilters) is still
 * used by the pages that pick a filter.
 * @returns API calls.
 */
export const useApiAdminFilterPage = () => {
  const api = useApi();

  return React.useRef({
    findFilterPage: (filter: IFilterPageFilter) => {
      return api.get<never, AxiosResponse<IPaged<IFilterModel>>, any>(
        `/admin/filters/page?${toQueryString(filter)}`,
      );
    },
  }).current;
};
