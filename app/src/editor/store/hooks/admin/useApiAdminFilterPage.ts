import { AxiosResponse } from 'axios';
import { type IFilterPageFilter } from '@/editor/features/admin/filters/interfaces';
import React from 'react';
import { type IFilterModel, type IPaged, toQueryString, useApi } from 'tno-core';

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
