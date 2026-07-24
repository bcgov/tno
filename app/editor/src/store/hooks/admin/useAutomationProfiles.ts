import {
  type IAutomationDebugRequestModel,
  type IAutomationDebugResultModel,
  type IAutomationProfileModel,
  type IAutomationRunDiffModel,
  type IAutomationRunModel,
  type IAutomationRunRequestModel,
} from 'features/admin/automation/interfaces';
import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { type IAdminState, useAdminStore } from 'store/slices';

import { useApiAdminAutomation } from './useApiAdminAutomation';

interface IAutomationProfileController {
  findProfiles: () => Promise<IAutomationProfileModel[]>;
  getProfile: (id: number) => Promise<IAutomationProfileModel>;
  addProfile: (model: IAutomationProfileModel) => Promise<IAutomationProfileModel>;
  updateProfile: (model: IAutomationProfileModel) => Promise<IAutomationProfileModel>;
  deleteProfile: (id: number) => Promise<IAutomationProfileModel>;
  runProfile: (id: number, request: IAutomationRunRequestModel) => Promise<IAutomationRunModel>;
  findRuns: (profileId?: number) => Promise<IAutomationRunModel[]>;
  getRunDiff: (runId: number) => Promise<IAutomationRunDiffModel>;
  debugContent: (
    id: number,
    request: IAutomationDebugRequestModel,
  ) => Promise<IAutomationDebugResultModel>;
}

export const useAutomationProfiles = (): [IAdminState, IAutomationProfileController] => {
  const api = useApiAdminAutomation();
  const dispatch = useAjaxWrapper();
  const [state] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findProfiles: async () => {
        const response = await dispatch<IAutomationProfileModel[]>(
          'find-automation-profiles',
          async () => await api.findProfiles(),
        );
        return response.data;
      },
      getProfile: async (id: number) => {
        const response = await dispatch<IAutomationProfileModel>(
          'get-automation-profile',
          async () => await api.getProfile(id),
        );
        return response.data;
      },
      addProfile: async (model: IAutomationProfileModel) => {
        const response = await dispatch<IAutomationProfileModel>(
          'add-automation-profile',
          async () => await api.addProfile(model),
        );
        return response.data;
      },
      updateProfile: async (model: IAutomationProfileModel) => {
        const response = await dispatch<IAutomationProfileModel>(
          'update-automation-profile',
          async () => await api.updateProfile(model),
        );
        return response.data;
      },
      deleteProfile: async (id: number) => {
        const response = await dispatch<IAutomationProfileModel>(
          'delete-automation-profile',
          async () => await api.deleteProfile(id),
        );
        return response.data;
      },
      runProfile: async (id: number, request: IAutomationRunRequestModel) => {
        const response = await dispatch<IAutomationRunModel>(
          'run-automation-profile',
          async () => await api.runProfile(id, request),
        );
        return response.data;
      },
      findRuns: async (profileId?: number) => {
        // Silent: polled for run status updates; must not trigger the page loading overlay.
        const response = await dispatch<IAutomationRunModel[]>(
          'find-automation-runs',
          async () => await api.findRuns(profileId),
          undefined,
          true,
        );
        return response.data;
      },
      getRunDiff: async (runId: number) => {
        // Silent: fetched inside the run detail modal; must not trigger the page loading overlay.
        const response = await dispatch<IAutomationRunDiffModel>(
          'get-automation-run-diff',
          async () => await api.getRunDiff(runId),
          undefined,
          true,
        );
        return response.data;
      },
      debugContent: async (id: number, request: IAutomationDebugRequestModel) => {
        const response = await dispatch<IAutomationDebugResultModel>(
          'debug-automation-content',
          async () => await api.debugContent(id, request),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [state, controller];
};
