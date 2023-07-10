import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

import { useApiMinisters } from './api/useApiMinisters';
import { IMinisterModel } from './interfaces/IMinisterModel';

interface IMinisterController {
  getMinisters: () => Promise<IMinisterModel[]>;
}

export const useMinisters = (): [IAdminState, IMinisterController] => {
  const api = useApiMinisters();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();

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
    // The state.mediaTypes will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [api, dispatch, store],
  );

  return [state, controller];
};
