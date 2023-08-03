import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IChartRequestModel, IChartResultModel, IChartTemplateModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminChartTemplates = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllChartTemplates: () => {
      return api.get<IChartTemplateModel[], AxiosResponse<IChartTemplateModel[]>, any>(
        `/admin/chart/templates`,
      );
    },
    getChartTemplate: (id: number) => {
      return api.get<IChartTemplateModel, AxiosResponse<IChartTemplateModel>, any>(
        `/admin/chart/templates/${id}`,
      );
    },
    addChartTemplate: (model: IChartTemplateModel) => {
      return api.post<IChartTemplateModel, AxiosResponse<IChartTemplateModel>, any>(
        `/admin/chart/templates`,
        model,
      );
    },
    updateChartTemplate: (model: IChartTemplateModel) => {
      return api.put<IChartTemplateModel, AxiosResponse<IChartTemplateModel>, any>(
        `/admin/chart/templates/${model.id}`,
        model,
      );
    },
    deleteChartTemplate: (model: IChartTemplateModel) => {
      return api.delete<IChartTemplateModel, AxiosResponse<IChartTemplateModel>, any>(
        `/admin/chart/templates/${model.id}`,
        {
          data: model,
        },
      );
    },
    previewJson: (model: IChartRequestModel) => {
      return api.post<IChartRequestModel, AxiosResponse<IChartResultModel>, any>(
        `/admin/chart/templates/preview/json`,
        model,
      );
    },
    previewBase64: (model: IChartRequestModel) => {
      return api.post<IChartRequestModel, AxiosResponse<string>, any>(
        `/admin/chart/templates/preview/base64`,
        model,
      );
    },
    previewImage: (model: IChartRequestModel) => {
      return api.post<IChartRequestModel, AxiosResponse<any>, any>(
        `/admin/chart/templates/preview/image`,
        model,
      );
    },
  }).current;
};
