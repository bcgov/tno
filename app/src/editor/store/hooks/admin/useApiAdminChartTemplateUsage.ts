import { AxiosResponse } from 'axios';
import React from 'react';

import { type IChartTemplateUsageModel } from '@/editor/features/admin/charts/interfaces';
import { useApi } from '@/shared';

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
