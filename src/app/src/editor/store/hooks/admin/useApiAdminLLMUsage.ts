import { AxiosResponse } from 'axios';
import React from 'react';

import {
  type ILLMMigrationCheckModel,
  type ILLMMigrationResultModel,
  type ILLMUsageModel,
} from '@/editor/features/admin/llms/interfaces';
import { useApi } from '@/shared';

export const useApiAdminLLMUsage = () => {
  const api = useApi();

  return React.useRef({
    getLLMUsage: (id: number) => {
      return api.get<never, AxiosResponse<ILLMUsageModel>, any>(`/admin/llms/${id}/usage`);
    },
    checkLLMMigration: (id: number, targetId: number) => {
      return api.get<never, AxiosResponse<ILLMMigrationCheckModel>, any>(
        `/admin/llms/${id}/migrate/${targetId}`,
      );
    },
    migrateLLM: (id: number, targetId: number) => {
      return api.post<never, AxiosResponse<ILLMMigrationResultModel>, any>(
        `/admin/llms/${id}/migrate/${targetId}`,
      );
    },
  }).current;
};
