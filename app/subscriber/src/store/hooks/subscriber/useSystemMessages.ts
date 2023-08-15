import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IProfileState, useProfileStore } from 'store/slices';
import { ISystemMessageModel, useApiSubscriberSystemMessages } from 'tno-core';

interface ISystemMessageController {
  findSystemMessage: () => Promise<ISystemMessageModel>;
}

export const useSystemMessages = (): [IProfileState, ISystemMessageController] => {
  const api = useApiSubscriberSystemMessages();
  const dispatch = useAjaxWrapper();
  const [state, store] = useProfileStore();

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
