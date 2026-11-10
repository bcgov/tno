import { AxiosResponse } from 'axios';
import { type IChartTemplateUsageModel } from 'features/admin/charts/interfaces';
import React from 'react';
import { useApi } from 'tno-core';

export const useApiAdminChartTemplateUsage = () => {
  const api = useApi();

  return React.useRef({
    getChartTemplateUsage: (id: number) => {
      return api.get<never, AxiosResponse<IChartTemplateUsageModel[]>, any>(
        `/admin/chart/templates/${id}/usage`,
      );
    },
  }).current;
};
