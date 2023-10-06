import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IAVOverviewInstanceModel, IReportResultModel, useApiEditorAVOverviews } from 'tno-core';

interface IAVOverviewController {
  findAVOverview: (publishedOn: Date | string) => Promise<IAVOverviewInstanceModel | undefined>;
  previewAVOverview: (instanceId: number) => Promise<IReportResultModel>;
}

export const useAVOverviewInstances = (): [IAVOverviewController] => {
  const api = useApiEditorAVOverviews();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      findAVOverview: async (publishedOn: Date | string) => {
        const response = await dispatch<IAVOverviewInstanceModel | undefined>(
          'find-av-overviews',
          () => api.findAVOverview(publishedOn),
        );
        return response.data;
      },
      previewAVOverview: async (instanceId: number) => {
        const response = await dispatch<IReportResultModel>('preview-av-overview', () =>
          api.previewAVOverview(instanceId),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
