import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { ISystemMessageModel, useApiSystemMessages } from 'tno-core';

interface ISystemMessageController {
  findSystemMessage: () => Promise<ISystemMessageModel>;
}

export const useSystemMessages = (): [IAdminState, ISystemMessageController] => {
  const api = useApiSystemMessages();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findSystemMessage: async () => {
        const response = await dispatch<ISystemMessageModel>('find-system-message', () =>
          api.findSystemMessage(),
        );
        store.storeSystemMessages([response.data]);
        return response.data;
      },
    }),
    [api, dispatch, store],
  );

  return [state, controller];
};
