import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import {
  type IAVOverviewInstanceModel,
  type IReportResultModel,
  useApiEditorAVOverviews,
} from 'tno-core';

interface IAVOverviewController {
  findAVOverview: (publishedOn: Date | string) => Promise<IAVOverviewInstanceModel | undefined>;
  getAVOverview: (id: number) => Promise<IAVOverviewInstanceModel | undefined>;
  addAVOverview: (model: IAVOverviewInstanceModel) => Promise<IAVOverviewInstanceModel>;
  updateAVOverview: (model: IAVOverviewInstanceModel) => Promise<IAVOverviewInstanceModel>;
  deleteAVOverview: (model: IAVOverviewInstanceModel) => Promise<IAVOverviewInstanceModel>;
  viewAVOverview: (instanceId: number) => Promise<IReportResultModel>;
  publishAVOverview: (instanceId: number) => Promise<IAVOverviewInstanceModel>;
}

export const useAVOverviewInstances = (): [IAVOverviewController] => {
  const api = useApiEditorAVOverviews();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      findAVOverview: async (publishedOn: Date | string) => {
        const response = await dispatch<IAVOverviewInstanceModel | undefined>(
          'find-av-overviews',
          async () => await api.findAVOverview(publishedOn),
        );
        return response.data;
      },
      getAVOverview: async (id: number) => {
        const response = await dispatch<IAVOverviewInstanceModel | undefined>(
          'get-av-overview',
          async () => await api.getAVOverview(id),
        );
        return response.data;
      },
      addAVOverview: async (model: IAVOverviewInstanceModel) => {
        const response = await dispatch<IAVOverviewInstanceModel>(
          'add-av-overview',
          async () => await api.addAVOverview(model),
        );
        return response.data;
      },
      updateAVOverview: async (model: IAVOverviewInstanceModel) => {
        const response = await dispatch<IAVOverviewInstanceModel>(
          'update-av-overview',
          async () => await api.updateAVOverview(model),
        );
        return response.data;
      },
      deleteAVOverview: async (model: IAVOverviewInstanceModel) => {
        const response = await dispatch<IAVOverviewInstanceModel>(
          'delete-av-overview',
          async () => await api.deleteAVOverview(model),
        );
        return response.data;
      },
      viewAVOverview: async (instanceId: number) => {
        const response = await dispatch<IReportResultModel>(
          'view-av-overview',
          async () => await api.viewAVOverview(instanceId),
        );
        return response.data;
      },
      publishAVOverview: async (instanceId: number) => {
        const response = await dispatch<IAVOverviewInstanceModel>(
          'publish-av-overview',
          async () => await api.publishAVOverview(instanceId),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
