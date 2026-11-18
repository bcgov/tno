import React from 'react';

import { IContributorModel, useApiSubscriberContributors } from '@/shared';
import { useAjaxWrapper } from '@/subscriber/store/hooks';
import { IProfileState, useProfileStore } from '@/subscriber/store/slices';

interface IContributorController {
  findAllContributors: () => Promise<IContributorModel[]>;
}

export const useContributors = (): [IProfileState, IContributorController] => {
  const api = useApiSubscriberContributors();
  const dispatch = useAjaxWrapper();
  const [state, store] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      findAllContributors: async () => {
        const response = await dispatch<IContributorModel[]>('find-all-contributors', () =>
          api.getContributors(),
        );
        store.storeContributors(response.data);
        return response.data;
      },
    }),
    [api, dispatch, store],
  );

  return [state, controller];
};
