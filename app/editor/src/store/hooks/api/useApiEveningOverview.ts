import { AxiosResponse } from 'axios';
import {
  IEveningOverviewItem,
  IEveningOverviewSection,
} from 'features/admin/evening-overview/interfaces';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, useApi } from 'tno-core';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiEveningOverviews = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllEveningOverviewSections: () => {
      return api.get<IEveningOverviewSection[], AxiosResponse<IEveningOverviewSection[]>, any>(
        `/editor/overview/sections`,
      );
    },
    findItemsBySectionId: (sectionId: number) => {
      return api.get<IEveningOverviewItem[], AxiosResponse<IEveningOverviewItem[]>, any>(
        `/editor/overview/section/items/group/${sectionId}`,
      );
    },
    addEveningOverviewSectionItem: (model: IEveningOverviewItem) => {
      return api.post<IEveningOverviewItem, AxiosResponse<IEveningOverviewItem>, any>(
        `/editor/overview/section/items`,
        model,
      );
    },
    updateEveningOverviewSectionItem: (model: IEveningOverviewItem) => {
      return api.put<IEveningOverviewItem, AxiosResponse<IEveningOverviewItem>, any>(
        `/editor/overview/section/items/${model.id}`,
        model,
      );
    },
    addEveningOverviewSection: (model: IEveningOverviewSection) => {
      return api.post<IEveningOverviewSection, AxiosResponse<IEveningOverviewSection>, any>(
        `/editor/overview/sections`,
        model,
      );
    },
    updateEveningOverviewSection: (model: IEveningOverviewSection) => {
      return api.put<IEveningOverviewSection, AxiosResponse<IEveningOverviewSection>, any>(
        `/editor/overview/sections/${model.id}`,
        model,
      );
    },
    deleteEveningOverviewSection: (model: IEveningOverviewSection) => {
      return api.delete<IEveningOverviewSection, AxiosResponse<IEveningOverviewSection>, any>(
        `/editor/overview/sections/${model.id}`,
        {
          data: model,
        },
      );
    },
    deleteEveningOverviewSectionItem: (model: IEveningOverviewItem) => {
      return api.delete<IEveningOverviewItem, AxiosResponse<IEveningOverviewItem>, any>(
        `/editor/overview/section/items/${model.id}`,
        {
          data: model,
        },
      );
    },
  }).current;
};
