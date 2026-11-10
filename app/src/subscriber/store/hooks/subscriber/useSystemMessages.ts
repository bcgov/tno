import React from 'react';
import { useAjaxWrapper } from '@/subscriber/store/hooks';
import { IProfileState, useProfileStore } from '@/subscriber/store/slices';
import { ISystemMessageModel, useApiSubscriberSystemMessages } from 'tno-core';

interface ISystemMessageController {
  findSystemMessages: () => Promise<ISystemMessageModel[]>;
}

export const useSystemMessages = (): [IProfileState, ISystemMessageController] => {
  const { findSystemMessages } = useApiSubscriberSystemMessages();
  const dispatch = useAjaxWrapper();
  const [state, store] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      findSystemMessages: async () => {
        const response = await dispatch<ISystemMessageModel[]>('find-system-messages', () =>
          findSystemMessages(),
        );
        store.storeMyMessages(response.data);
        return response.data;
      },
    }),
    [findSystemMessages, dispatch, store],
  );

  return [state, controller];
};
