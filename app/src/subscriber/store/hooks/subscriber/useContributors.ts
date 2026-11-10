import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IProfileState, useProfileStore } from 'store/slices';
import { IContributorModel, useApiSubscriberContributors } from 'tno-core';

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
