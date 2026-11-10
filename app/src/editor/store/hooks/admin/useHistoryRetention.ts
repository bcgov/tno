import { type IHistoryPurgePreviewModel } from 'features/admin/settings/interfaces';
import React from 'react';
import { useAjaxWrapper } from 'store/hooks';

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
