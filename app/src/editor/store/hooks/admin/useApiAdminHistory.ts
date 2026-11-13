import { AxiosResponse } from 'axios';
import React from 'react';

import { type IHistoryPurgePreviewModel } from '@/editor/features/admin/settings/interfaces';
import { useApi } from '@/shared';

export const useApiAdminHistory = () => {
  const api = useApi();

  return React.useRef({
    previewHistoryPurge: () => {
      return api.get<never, AxiosResponse<IHistoryPurgePreviewModel>, any>(
        '/admin/history/purge/preview',
      );
    },
  }).current;
};
