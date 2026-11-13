import React from 'react';

import { type IHistoryPurgePreviewModel } from '@/editor/features/admin/settings/interfaces';
import { useAjaxWrapper } from '@/editor/store/hooks';

import { useApiAdminHistory } from './useApiAdminHistory';

interface IHistoryRetentionController {
  previewHistoryPurge: () => Promise<IHistoryPurgePreviewModel>;
}

export const useHistoryRetention = (): IHistoryRetentionController => {
  const api = useApiAdminHistory();
  const dispatch = useAjaxWrapper();

  return React.useMemo(
    () => ({
      previewHistoryPurge: async () => {
        const response = await dispatch<IHistoryPurgePreviewModel>(
          'preview-history-purge',
          async () => await api.previewHistoryPurge(),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );
};
