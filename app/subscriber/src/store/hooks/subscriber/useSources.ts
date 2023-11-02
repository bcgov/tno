import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IProfileState, useProfileStore } from 'store/slices';
import { ISourceModel, useApiSubscriberSources } from 'tno-core';

interface ISourceController {
  findAllSources: () => Promise<ISourceModel[]>;
}

export const useSources = (): [IProfileState, ISourceController] => {
  const api = useApiSubscriberSources();
  const dispatch = useAjaxWrapper();
  const [state, store] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      findAllSources: async () => {
        const response = await dispatch<ISourceModel[]>('find-all-sources', () =>
          api.findAllSources(),
        );
        store.storeSources(response.data);
        return response.data;
      },
    }),
    [dispatch, store, api],
  );

  return [state, controller];
};
