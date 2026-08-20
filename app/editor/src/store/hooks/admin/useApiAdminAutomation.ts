import { AxiosResponse } from 'axios';
import {
  type IAutomationDebugRequestModel,
  type IAutomationDebugResultModel,
  type IAutomationProfileModel,
  type IAutomationRunDiffModel,
  type IAutomationRunModel,
  type IAutomationRunRequestModel,
} from 'features/admin/automation/interfaces';
import {
  type IAutomationExplainRequestModel,
  type IAutomationExplainResultModel,
  type IAutomationRunLogFilter,
  type IAutomationRunLogPage,
  type IV2ActionDescriptor,
  type IV2MigrateResultModel,
  type IV2ValidationError,
} from 'features/admin/automation/v2/interfaces';
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
    getV2Descriptors: () => {
      return api.get<never, AxiosResponse<IV2ActionDescriptor[]>, any>(
        '/admin/automation/v2/descriptors',
      );
    },
    validateProfile: (model: IAutomationProfileModel) => {
      return api.post<IAutomationProfileModel, AxiosResponse<IV2ValidationError[]>, any>(
        '/admin/automation/profiles/validate',
        model,
      );
    },
    migrateProfile: (id: number) => {
      return api.post<never, AxiosResponse<IV2MigrateResultModel>, any>(
        `/admin/automation/profiles/${id}/migrate`,
      );
    },
    findRunLogs: (runId: number, filter: IAutomationRunLogFilter) => {
      const params = new URLSearchParams();
      if (filter.step) params.set('step', filter.step);
      if (filter.action) params.set('action', filter.action);
      if (filter.outcome) params.set('outcome', filter.outcome);
      if (filter.contentId) params.set('contentId', `${filter.contentId}`);
      if (filter.search) params.set('search', filter.search);
      params.set('page', `${filter.page ?? 1}`);
      params.set('qty', `${filter.qty ?? 100}`);
      if (filter.direction) params.set('direction', filter.direction);
      return api.get<never, AxiosResponse<IAutomationRunLogPage>, any>(
        `/admin/automation/runs/${runId}/logs?${params.toString()}`,
      );
    },
    explainRunLog: (runId: number, logId: number, request: IAutomationExplainRequestModel) => {
      return api.post<
        IAutomationExplainRequestModel,
        AxiosResponse<IAutomationExplainResultModel>,
        any
      >(`/admin/automation/runs/${runId}/logs/${logId}/explain`, request);
    },
  }).current;
};
