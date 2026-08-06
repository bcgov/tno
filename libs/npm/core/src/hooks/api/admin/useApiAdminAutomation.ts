import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import {
  IAutomationProfileModel,
  IAutomationRunModel,
  IAutomationRunRequestModel,
  useApi,
} from '..';

export const useApiAdminAutomation = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findProfiles: () => {
      return api.get<never, AxiosResponse<IAutomationProfileModel[]>, any>(
        '/admin/automation/profiles',
      );
    },
    getProfile: (id: number) => {
      return api.get<never, AxiosResponse<IAutomationProfileModel>, any>(
        `/admin/automation/profiles/${id}`,
      );
    },
    addProfile: (model: IAutomationProfileModel) => {
      return api.post<IAutomationProfileModel, AxiosResponse<IAutomationProfileModel>, any>(
        '/admin/automation/profiles',
        model,
      );
    },
    updateProfile: (model: IAutomationProfileModel) => {
      return api.put<IAutomationProfileModel, AxiosResponse<IAutomationProfileModel>, any>(
        `/admin/automation/profiles/${model.id}`,
        model,
      );
    },
    deleteProfile: (id: number) => {
      return api.delete<IAutomationProfileModel, AxiosResponse<IAutomationProfileModel>, any>(
        `/admin/automation/profiles/${id}`,
      );
    },
    runProfile: (id: number, request: IAutomationRunRequestModel) => {
      return api.post<IAutomationRunModel, AxiosResponse<IAutomationRunModel>, any>(
        `/admin/automation/profiles/${id}/run`,
        request,
      );
    },
    findRuns: (profileId?: number) => {
      const query = profileId ? `?profileId=${profileId}` : '';
      return api.get<never, AxiosResponse<IAutomationRunModel[]>, any>(
        `/admin/automation/runs${query}`,
      );
    },
  }).current;
};
