import React from 'react';
import { useAjaxWrapper } from '@/editor/store/hooks';
import { type IAdminState, useAdminStore } from '@/editor/store/slices';
import { type ISystemMessageModel, useApiEditorSystemMessages } from 'tno-core';

interface ISystemMessageController {
  findSystemMessages: () => Promise<ISystemMessageModel[]>;
}

export const useSystemMessages = (): [IAdminState, ISystemMessageController] => {
  const api = useApiEditorSystemMessages();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findSystemMessages: async () => {
        const response = await dispatch<ISystemMessageModel[]>(
          'find-system-messages',
          async () => await api.findSystemMessages(),
        );
        store.storeSystemMessages(response.data);
        return response.data;
      },
    }),
    [api, dispatch, store],
  );

  return [state, controller];
};
