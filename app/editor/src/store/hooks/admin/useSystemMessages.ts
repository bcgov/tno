import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { ISystemMessageModel, useApiAdminSystemMessages } from 'tno-core';

interface IAlertController {
  findSystemMessage: () => Promise<ISystemMessageModel>;
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
      findSystemMessage: async () => {
        const response = await dispatch<ISystemMessageModel>('find-system-message', () =>
          api.findSystemMessage(),
        );
        store.storeSystemMessages([response.data]);
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
