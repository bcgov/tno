import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IProfileState, useProfileStore } from 'store/slices';
import { useApiSubscriberMinisters } from 'tno-core';

import { IMinisterModel } from './interfaces/IMinisterModel';

interface IMinisterController {
  getMinisters: () => Promise<IMinisterModel[]>;
}

export const useMinisters = (): [IProfileState, IMinisterController] => {
  const api = useApiSubscriberMinisters();
  const dispatch = useAjaxWrapper();
  const [state, store] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      getMinisters: async () => {
        const response = await dispatch<IMinisterModel[]>('find-all-ministers', () =>
          api.getMinisters(),
        );
        store.storeMyMinisters(response.data);
        return response.data;
      },
    }),
    [api, dispatch, store],
  );

  return [state, controller];
};
