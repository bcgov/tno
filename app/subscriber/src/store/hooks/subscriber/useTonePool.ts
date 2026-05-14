import React from 'react';
import { IProfileState, useProfileStore } from 'store/slices';
import { ITonePoolModel, useApiSubscriberTonePools } from 'tno-core';

import { useAjaxWrapper } from '../useAjaxWrapper';

interface ITonePoolController {
  addMyTonePool: (model: ITonePoolModel) => Promise<ITonePoolModel | undefined>;
  getMyTonePool: (userId: number) => Promise<ITonePoolModel | undefined>;
}

export const useTonePool = (): [IProfileState, ITonePoolController] => {
  const api = useApiSubscriberTonePools();
  const dispatch = useAjaxWrapper();
  const [state, { storeMyTonePool }] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      addMyTonePool: async (model: ITonePoolModel) => {
        const response = await dispatch<ITonePoolModel>('add-my-tonePool', () =>
          api.addMyTonePool(model),
        );
        if (response.status === 201) {
          storeMyTonePool(response.data as ITonePoolModel | undefined);
        }

        return response.data as ITonePoolModel | undefined;
      },
      getMyTonePool: async (userId: number) => {
        const response = await dispatch('get-my-tonePool', () => api.getMyTonePool(userId));
        storeMyTonePool(response.data as ITonePoolModel | undefined);
        return response.data as ITonePoolModel | undefined;
      },
    }),
    [api, dispatch, storeMyTonePool],
  );

  return [state, controller];
};
