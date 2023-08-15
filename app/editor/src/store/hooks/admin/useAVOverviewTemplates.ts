import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import {
  AVOverviewTemplateTypeName,
  IAVOverviewTemplateModel,
  useApiAdminAVOverviews,
} from 'tno-core';

interface IAVOverviewController {
  findAllAVOverview: () => Promise<IAVOverviewTemplateModel[]>;
  getAVOverview: (
    templateType: AVOverviewTemplateTypeName,
  ) => Promise<IAVOverviewTemplateModel | undefined>;
  addAVOverview: (model: IAVOverviewTemplateModel) => Promise<IAVOverviewTemplateModel>;
  updateAVOverview: (model: IAVOverviewTemplateModel) => Promise<IAVOverviewTemplateModel>;
  deleteAVOverview: (model: IAVOverviewTemplateModel) => Promise<IAVOverviewTemplateModel>;
}

export const useAVOverviewTemplates = (): [IAVOverviewController] => {
  const api = useApiAdminAVOverviews();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      findAllAVOverview: async () => {
        const response = await dispatch<IAVOverviewTemplateModel[]>('find-av-overviews', () =>
          api.findAllAVOverview(),
        );
        return response.data;
      },
      getAVOverview: async (templateType: AVOverviewTemplateTypeName) => {
        const response = await dispatch<IAVOverviewTemplateModel | undefined>(
          'get-av-overview',
          () => api.getAVOverview(templateType),
        );
        return response.data;
      },
      addAVOverview: async (model: IAVOverviewTemplateModel) => {
        const response = await dispatch<IAVOverviewTemplateModel>('add-av-overview', () =>
          api.addAVOverview(model),
        );
        return response.data;
      },
      updateAVOverview: async (model: IAVOverviewTemplateModel) => {
        const response = await dispatch<IAVOverviewTemplateModel>('update-av-overview', () =>
          api.updateAVOverview(model),
        );
        return response.data;
      },
      deleteAVOverview: async (model: IAVOverviewTemplateModel) => {
        const response = await dispatch<IAVOverviewTemplateModel>('delete-av-overview', () =>
          api.deleteAVOverview(model),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
