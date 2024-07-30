import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { ISystemMessageModel, useApiAdminSystemMessages } from 'tno-core';

interface IAlertController {
  findSystemMessages: () => Promise<ISystemMessageModel[]>;
  findSystemMessage: (id: number) => Promise<ISystemMessageModel>;
  addSystemMessage: (model: ISystemMessageModel) => Promise<ISystemMessageModel>;
  updateSystemMessage: (model: ISystemMessageModel) => Promise<ISystemMessageModel>;
  deleteSystemMessage: (model: ISystemMessageModel) => Promise<ISystemMessageModel>;
}

export const useSystemMessages = (): [IAdminState, IAlertController] => {
  const api = useApiAdminSystemMessages();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findSystemMessages: async () => {
        const response = await dispatch<ISystemMessageModel[]>('find-system-messages', () =>
          api.findSystemMessages(),
        );
        store.storeSystemMessages(response.data);
        return response.data;
      },
      findSystemMessage: async (id: number) => {
        const response = await dispatch<ISystemMessageModel>('find-system-message', () =>
          api.findSystemMessage(id),
        );
        store.storeSystemMessages((messages) => {
          const found = messages.some((m) => m.id === id);
          if (found) return messages.map((m) => (m.id === id ? response.data : m));
          return [...messages, response.data];
        });
        return response.data;
      },
      addSystemMessage: async (model: ISystemMessageModel) => {
        const response = await dispatch<ISystemMessageModel>('add-system-message', () =>
          api.addSystemMessage(model),
        );
        store.storeSystemMessages((systemMessages) => [...systemMessages, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateSystemMessage: async (model: ISystemMessageModel) => {
        const response = await dispatch<ISystemMessageModel>('update-system-message', () =>
          api.updateSystemMessage(model),
        );
        store.storeSystemMessages((systemMessages) =>
          systemMessages.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteSystemMessage: async (model: ISystemMessageModel) => {
        const response = await dispatch<ISystemMessageModel>('delete-system-message', () =>
          api.deleteSystemMessage(model),
        );
        store.storeTags((tags) => tags.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [state, controller];
};
