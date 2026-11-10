import React from 'react';
import { useApiEditorSystemMessages, type ISystemMessageModel } from 'tno-core';

import { useAjaxWrapper } from '@/editor/store/hooks';
import { useAdminStore, type IAdminState } from '@/editor/store/slices';

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
