import { AxiosResponse } from 'axios';
import {
  type IAnalysisBackfillModel,
  type IAnalysisBackfillPreviewModel,
  type IAnalysisBackfillRequestModel,
  type IAnalysisFailureModel,
  type IAnalysisQueueModel,
  type IContentAnalysisSettingsModel,
} from 'features/admin/content-analysis/interfaces';
import { type IAnalysisRequestModel } from 'features/content/form/interfaces';
import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { useApi } from 'tno-core';

export interface IContentAnalysisAdminController {
  getSettings: () => Promise<IContentAnalysisSettingsModel>;
  updateSettings: (model: IContentAnalysisSettingsModel) => Promise<IContentAnalysisSettingsModel>;
  getQueue: () => Promise<IAnalysisQueueModel>;
  findFailures: () => Promise<IAnalysisFailureModel[]>;
  replay: (contentId: number) => Promise<IAnalysisRequestModel>;
  previewBackfill: (model: IAnalysisBackfillRequestModel) => Promise<IAnalysisBackfillPreviewModel>;
  startBackfill: (model: IAnalysisBackfillRequestModel) => Promise<IAnalysisBackfillModel>;
  findBackfills: () => Promise<IAnalysisBackfillModel[]>;
  cancelBackfill: (id: number) => Promise<IAnalysisBackfillModel>;
  resumeBackfill: (id: number) => Promise<IAnalysisBackfillModel>;
}

/**
 * Content-Analysis administration.
 * @returns Controller.
 */
export const useContentAnalysisAdmin = (): IContentAnalysisAdminController => {
  const api = useApi();
  const dispatch = useAjaxWrapper();

  return React.useMemo(
    () => ({
      getSettings: async () =>
        (
          await dispatch('get-analysis-settings', () =>
            api.get<never, AxiosResponse<IContentAnalysisSettingsModel>, any>(
              '/admin/analysis/settings',
            ),
          )
        ).data,
      updateSettings: async (model: IContentAnalysisSettingsModel) =>
        (
          await dispatch('update-analysis-settings', () =>
            api.put<
              IContentAnalysisSettingsModel,
              AxiosResponse<IContentAnalysisSettingsModel>,
              any
            >('/admin/analysis/settings', model),
          )
        ).data,
      getQueue: async () =>
        (
          await dispatch(
            'get-analysis-queue',
            () => api.get<never, AxiosResponse<IAnalysisQueueModel>, any>('/admin/analysis/queue'),
            undefined,
            true,
          )
        ).data,
      findFailures: async () =>
        (
          await dispatch('find-analysis-failures', () =>
            api.get<never, AxiosResponse<IAnalysisFailureModel[]>, any>('/admin/analysis/failures'),
          )
        ).data,
      replay: async (contentId: number) =>
        (
          await dispatch('replay-analysis', () =>
            api.post<never, AxiosResponse<IAnalysisRequestModel>, any>(
              `/admin/analysis/contents/${contentId}/replay`,
            ),
          )
        ).data,
      previewBackfill: async (model: IAnalysisBackfillRequestModel) =>
        (
          await dispatch('preview-analysis-backfill', () =>
            api.post<
              IAnalysisBackfillRequestModel,
              AxiosResponse<IAnalysisBackfillPreviewModel>,
              any
            >('/admin/analysis/backfills/preview', model),
          )
        ).data,
      startBackfill: async (model: IAnalysisBackfillRequestModel) =>
        (
          await dispatch('start-analysis-backfill', () =>
            api.post<IAnalysisBackfillRequestModel, AxiosResponse<IAnalysisBackfillModel>, any>(
              '/admin/analysis/backfills',
              model,
            ),
          )
        ).data,
      findBackfills: async () =>
        (
          await dispatch(
            'find-analysis-backfills',
            () =>
              api.get<never, AxiosResponse<IAnalysisBackfillModel[]>, any>(
                '/admin/analysis/backfills',
              ),
            undefined,
            true,
          )
        ).data,
      cancelBackfill: async (id: number) =>
        (
          await dispatch('cancel-analysis-backfill', () =>
            api.post<never, AxiosResponse<IAnalysisBackfillModel>, any>(
              `/admin/analysis/backfills/${id}/cancel`,
            ),
          )
        ).data,
      resumeBackfill: async (id: number) =>
        (
          await dispatch('resume-analysis-backfill', () =>
            api.post<never, AxiosResponse<IAnalysisBackfillModel>, any>(
              `/admin/analysis/backfills/${id}/resume`,
            ),
          )
        ).data,
    }),
    [api, dispatch],
  );
};
