import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, IReportSectionSettingsModel, useApi } from 'tno-core';

import {
  IReportAIPromptChatRequestModel,
  IReportAIPromptChatResultModel,
  IReportAISectionPromptModel,
  IReportAISectionTestResultModel,
} from './interfaces';

/**
 * Endpoints that show, try and discuss the prompt of a report's AI section.
 * @param options Configuration options.
 * @returns Api methods.
 */
export const useApiSubscriberReportAI = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    describeSection: (
      reportId: number,
      sectionId: number,
      settings: IReportSectionSettingsModel,
    ) => {
      return api.post<
        IReportAISectionPromptModel,
        AxiosResponse<IReportAISectionPromptModel>,
        IReportSectionSettingsModel
      >(`/subscriber/reports/${reportId}/sections/${sectionId}/ai/prompt`, settings);
    },
    describeInstanceSection: (instanceId: number, sectionId: number) => {
      return api.get<
        IReportAISectionPromptModel,
        AxiosResponse<IReportAISectionPromptModel>,
        never
      >(`/subscriber/report/instances/${instanceId}/ai-sections/${sectionId}/prompt`);
    },
    testSection: (reportId: number, sectionId: number, settings: IReportSectionSettingsModel) => {
      return api.post<
        IReportAISectionTestResultModel,
        AxiosResponse<IReportAISectionTestResultModel>,
        IReportSectionSettingsModel
      >(`/subscriber/reports/${reportId}/sections/${sectionId}/ai/test`, settings);
    },
    getTest: (reportId: number, sectionId: number, testId: string) => {
      return api.get<
        IReportAISectionTestResultModel | undefined,
        AxiosResponse<IReportAISectionTestResultModel | undefined>,
        never
      >(`/subscriber/reports/${reportId}/sections/${sectionId}/ai/tests/${testId}`);
    },
    chat: (reportId: number, sectionId: number, request: IReportAIPromptChatRequestModel) => {
      return api.post<
        IReportAIPromptChatResultModel,
        AxiosResponse<IReportAIPromptChatResultModel>,
        IReportAIPromptChatRequestModel
      >(`/subscriber/reports/${reportId}/sections/${sectionId}/ai/chat`, request);
    },
  }).current;
};
