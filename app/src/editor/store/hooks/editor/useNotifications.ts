import React from 'react';
import { useApiEditorNotifications } from 'tno-core';

import { useAjaxWrapper } from '@/editor/store/hooks';

interface INotificationController {
  publishNotification: (notificationId: number) => Promise<never>;
}

export const useNotifications = (): [INotificationController] => {
  const api = useApiEditorNotifications();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      publishNotification: async (notificationId: number) => {
        const response = await dispatch<never>(
          'publish-notification',
          async () => await api.publishNotification(notificationId),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
