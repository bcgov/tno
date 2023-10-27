import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { INotificationTemplateModel, useApiAdminNotificationTemplates } from 'tno-core';

interface INotificationTemplateController {
  findAllNotificationTemplates: () => Promise<INotificationTemplateModel[]>;
  getNotificationTemplate: (id: number) => Promise<INotificationTemplateModel>;
  addNotificationTemplate: (
    model: INotificationTemplateModel,
  ) => Promise<INotificationTemplateModel>;
  updateNotificationTemplate: (
    model: INotificationTemplateModel,
  ) => Promise<INotificationTemplateModel>;
  deleteNotificationTemplate: (
    model: INotificationTemplateModel,
  ) => Promise<INotificationTemplateModel>;
}

export const useNotificationTemplates = (): [
  IAdminState & { initialized: boolean },
  INotificationTemplateController,
] => {
  const api = useApiAdminNotificationTemplates();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();
  const [initialized, setInitialized] = React.useState(false);

  const controller = React.useMemo(
    () => ({
      findAllNotificationTemplates: async () => {
        const response = await dispatch<INotificationTemplateModel[]>(
          'find-all-notification-templates',
          () => api.findAllNotificationTemplates(),
        );
        store.storeNotificationTemplates(response.data);
        setInitialized(true);
        return response.data;
      },
      getNotificationTemplate: async (id: number) => {
        const response = await dispatch<INotificationTemplateModel>(
          'get-notification-template',
          () => api.getNotificationTemplate(id),
        );
        store.storeNotificationTemplates((notificationTemplates) =>
          notificationTemplates.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addNotificationTemplate: async (model: INotificationTemplateModel) => {
        const response = await dispatch<INotificationTemplateModel>(
          'add-notification-template',
          () => api.addNotificationTemplate(model),
        );
        store.storeNotificationTemplates((notificationTemplates) => [
          ...notificationTemplates,
          response.data,
        ]);
        await lookup.getLookups();
        return response.data;
      },
      updateNotificationTemplate: async (model: INotificationTemplateModel) => {
        const response = await dispatch<INotificationTemplateModel>(
          'update-notification-template',
          () => api.updateNotificationTemplate(model),
        );
        store.storeNotificationTemplates((notificationTemplates) =>
          notificationTemplates.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteNotificationTemplate: async (model: INotificationTemplateModel) => {
        const response = await dispatch<INotificationTemplateModel>(
          'delete-notification-template',
          () => api.deleteNotificationTemplate(model),
        );
        store.storeNotificationTemplates((notificationTemplates) =>
          notificationTemplates.filter((ds) => ds.id !== response.data.id),
        );
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [{ ...state, initialized }, controller];
};
