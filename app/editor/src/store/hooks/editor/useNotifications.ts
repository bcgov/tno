import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { useApiEditorNotifications } from 'tno-core';

interface INotificationController {
  publishNotification: (notificationId: number) => Promise<never>;
}

export const useNotifications = (): [INotificationController] => {
  const api = useApiEditorNotifications();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      publishNotification: async (notificationId: number) => {
        const response = await dispatch<never>('publish-notification', () =>
          api.publishNotification(notificationId),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
