import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { INotificationModel, useApiAdminNotifications } from 'tno-core';

interface INotificationController {
  findAllNotifications: () => Promise<INotificationModel[]>;
  getNotification: (id: number) => Promise<INotificationModel>;
  addNotification: (model: INotificationModel) => Promise<INotificationModel>;
  updateNotification: (model: INotificationModel) => Promise<INotificationModel>;
  deleteNotification: (model: INotificationModel) => Promise<INotificationModel>;
  sendNotification: (
    model: INotificationModel,
    contentId: number,
    to: string,
  ) => Promise<INotificationModel>;
}

export const useNotifications = (): [IAdminState, INotificationController] => {
  const api = useApiAdminNotifications();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllNotifications: async () => {
        const response = await dispatch<INotificationModel[]>('find-all-Notifications', () =>
          api.findAllNotifications(),
        );
        store.storeNotifications(response.data);
        return response.data;
      },
      getNotification: async (id: number) => {
        const response = await dispatch<INotificationModel>('get-Notification', () =>
          api.getNotification(id),
        );
        store.storeNotifications((Notifications) =>
          Notifications.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addNotification: async (model: INotificationModel) => {
        const response = await dispatch<INotificationModel>('add-Notification', () =>
          api.addNotification(model),
        );
        store.storeNotifications((Notifications) => [...Notifications, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateNotification: async (model: INotificationModel) => {
        const response = await dispatch<INotificationModel>('update-Notification', () =>
          api.updateNotification(model),
        );
        store.storeNotifications((Notifications) =>
          Notifications.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteNotification: async (model: INotificationModel) => {
        const response = await dispatch<INotificationModel>('delete-Notification', () =>
          api.deleteNotification(model),
        );
        store.storeNotifications((Notifications) =>
          Notifications.filter((ds) => ds.id !== response.data.id),
        );
        await lookup.getLookups();
        return response.data;
      },
      sendNotification: async (model: INotificationModel, contentId: number, to: string) => {
        const response = await dispatch<INotificationModel>('send-Notification', () =>
          api.sendNotification(model, contentId, to),
        );
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [state, controller];
};
