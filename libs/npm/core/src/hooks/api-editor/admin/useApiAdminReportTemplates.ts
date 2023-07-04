import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IReportTemplateModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminReportTemplates = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllReportTemplates: () => {
      return api.get<IReportTemplateModel[], AxiosResponse<IReportTemplateModel[]>, any>(
        `/admin/report/templates`,
      );
    },
    getReportTemplate: (id: number) => {
      return api.get<IReportTemplateModel, AxiosResponse<IReportTemplateModel>, any>(
        `/admin/report/templates/${id}`,
      );
    },
    addReportTemplate: (model: IReportTemplateModel) => {
      return api.post<IReportTemplateModel, AxiosResponse<IReportTemplateModel>, any>(
        `/admin/report/templates`,
        model,
      );
    },
    updateReportTemplate: (model: IReportTemplateModel) => {
      return api.put<IReportTemplateModel, AxiosResponse<IReportTemplateModel>, any>(
        `/admin/report/templates/${model.id}`,
        model,
      );
    },
    deleteReportTemplate: (model: IReportTemplateModel) => {
      return api.delete<IReportTemplateModel, AxiosResponse<IReportTemplateModel>, any>(
        `/admin/report/templates/${model.id}`,
        {
          data: model,
        },
      );
    },
  }).current;
};
