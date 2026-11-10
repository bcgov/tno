import { AxiosResponse } from 'axios';
import { type IHistoryPurgePreviewModel } from 'features/admin/settings/interfaces';
import React from 'react';
import { useApi } from 'tno-core';

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
