import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import {
  IDashboardFilter,
  INotificationFilter,
  INotificationInstanceModel,
  INotificationModel,
  INotificationResultModel,
  useApiAdminNotifications,
} from 'tno-core';

interface INotificationController {
  findNotifications: (filter?: INotificationFilter) => Promise<INotificationModel[]>;
  getNotification: (id: number) => Promise<INotificationModel>;
  addNotification: (model: INotificationModel) => Promise<INotificationModel>;
  updateNotification: (model: INotificationModel) => Promise<INotificationModel>;
  deleteNotification: (model: INotificationModel) => Promise<INotificationModel>;
  previewNotification: (
    model: INotificationModel,
    contentId: number,
  ) => Promise<INotificationResultModel>;
  sendNotification: (
    model: INotificationModel,
    to: string,
    contentId?: number,
  ) => Promise<INotificationModel>;
  getDashboard: (filter: IDashboardFilter) => Promise<INotificationInstanceModel[]>;
}

export const useNotifications = (): [IAdminState, INotificationController] => {
  const api = useApiAdminNotifications();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findNotifications: async (filter?: INotificationFilter) => {
        const response = await dispatch<INotificationModel[]>('find-Notifications', () =>
          api.findNotifications(filter),
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
      previewNotification: async (model: INotificationModel, contentId: number) => {
        const response = await dispatch<INotificationResultModel>('preview-Notification', () =>
          api.previewNotification(model, contentId),
        );
        return response.data;
      },
      sendNotification: async (model: INotificationModel, to: string, contentId?: number) => {
        const response = await dispatch<INotificationModel>('send-Notification', () =>
          api.sendNotification(model, to, contentId),
        );
        return response.data;
      },
      getDashboard: async (filter: IDashboardFilter) => {
        const response = await dispatch<INotificationInstanceModel[]>('get-dashboard', () =>
          api.getDashboard(filter),
        );
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [state, controller];
};
