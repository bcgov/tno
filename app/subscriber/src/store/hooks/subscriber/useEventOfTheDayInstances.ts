import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IAVOverviewInstanceModel, useApiSubscriberAVOverviews } from 'tno-core';

interface IEventOfTheDayController {
  findAVOverview: (
    publishedOn?: Date | string | null,
  ) => Promise<IAVOverviewInstanceModel | undefined>;
  viewAVOverview: (instanceId: number) => Promise<never>;
}

export const useEventOfTheDayInstances = (): [IEventOfTheDayController] => {
  const api = useApiSubscriberAVOverviews();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      findAVOverview: async (publishedOn?: Date | string | null) => {
        const response = await dispatch<IAVOverviewInstanceModel | undefined>(
          'find-av-overviews',
          () => api.findAVOverview(publishedOn),
        );
        return response.data;
      },
      viewAVOverview: async (instanceId: number) => {
        const response = await dispatch<never>('view-av-overview', () =>
          api.viewAVOverview(instanceId),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
