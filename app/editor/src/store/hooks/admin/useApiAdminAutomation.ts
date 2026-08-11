import { AxiosResponse } from 'axios';
import {
  type IAutomationDebugRequestModel,
  type IAutomationDebugResultModel,
  type IAutomationProfileModel,
  type IAutomationRunDiffModel,
  type IAutomationRunModel,
  type IAutomationRunRequestModel,
} from 'features/admin/automation/interfaces';
import React from 'react';
import { useApi } from 'tno-core';

export const useApiAdminAutomation = () => {
  const api = useApi();

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
    getRunDiff: (runId: number) => {
      return api.get<never, AxiosResponse<IAutomationRunDiffModel>, any>(
        `/admin/automation/runs/${runId}/diff`,
      );
    },
    deleteRun: (runId: number) => {
      return api.delete<never, AxiosResponse<IAutomationRunModel>, any>(
        `/admin/automation/runs/${runId}`,
      );
    },
    debugContent: (id: number, request: IAutomationDebugRequestModel) => {
      return api.post<
        IAutomationDebugRequestModel,
        AxiosResponse<IAutomationDebugResultModel>,
        any
      >(`/admin/automation/profiles/${id}/debug`, request);
    },
    clearScheduleLastRun: (profileId: number, scheduleId: number) => {
      return api.post<never, AxiosResponse<void>, any>(
        `/admin/automation/profiles/${profileId}/schedules/${scheduleId}/clear-last-run`,
      );
    },
  }).current;
};
