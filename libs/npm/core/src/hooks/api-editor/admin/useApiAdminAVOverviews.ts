import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import {
  AVOverviewTemplateType,
  AVOverviewTemplateTypeName,
  IAVOverviewTemplateModel,
  useApi,
} from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminAVOverviews = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllAVOverview: () => {
      return api.get<never, AxiosResponse<IAVOverviewTemplateModel[]>, any>(`/admin/av/overviews`);
    },
    getAVOverview: (type: AVOverviewTemplateType | AVOverviewTemplateTypeName) => {
      return api.get<never, AxiosResponse<IAVOverviewTemplateModel | undefined>, any>(
        `/admin/av/overviews/${type}`,
      );
    },
    addAVOverview: (template: IAVOverviewTemplateModel) => {
      return api.post<IAVOverviewTemplateModel, AxiosResponse<IAVOverviewTemplateModel>, any>(
        '/admin/av/overviews',
        template,
      );
    },
    updateAVOverview: (template: IAVOverviewTemplateModel) => {
      return api.put<IAVOverviewTemplateModel, AxiosResponse<IAVOverviewTemplateModel>, any>(
        `/admin/av/overviews/${template.templateType}`,
        template,
      );
    },
    deleteAVOverview: (template: IAVOverviewTemplateModel) => {
      return api.delete<IAVOverviewTemplateModel, AxiosResponse<IAVOverviewTemplateModel>, any>(
        `/admin/av/overviews/${template.templateType}`,
        { data: template },
      );
    },
  }).current;
};
