import React from 'react';
import { useApiSubscriberMinisters } from 'tno-core';

import { useAjaxWrapper } from '@/subscriber/store/hooks';
import { useLookupStore } from '@/subscriber/store/slices';

import { IMinisterModel } from './interfaces/IMinisterModel';

interface IMinisterController {
  getMinisters: () => Promise<IMinisterModel[]>;
}

export const useMinisters = (): [IMinisterModel[], IMinisterController] => {
  const api = useApiSubscriberMinisters();
  const dispatch = useAjaxWrapper();
  // const [state, store] = useProfileStore();
  const [{ ministers }, store] = useLookupStore();

  const controller = React.useMemo(
    () => ({
      getMinisters: async () => {
        const response = await dispatch<IMinisterModel[]>('find-all-ministers', () =>
          api.getMinisters(),
        );
        store.storeMinisters(response.data);
        return response.data;
      },
    }),
    [api, dispatch, store],
  );

  return [ministers, controller];
};
