import { AxiosResponse } from 'axios';
import {
  type ITopicRescoreJobModel,
  type ITopicRescorePreviewModel,
  type ITopicRescoreRequestModel,
  type ITopicScoreSourceModel,
  type ITopicScoreTestRequestModel,
  type ITopicScoreTestResultModel,
} from 'features/admin/topic-score-rules/interfaces';
import React from 'react';
import { type ITopicScoreRuleModel, useApi } from 'tno-core';

export const useApiAdminTopicScores = () => {
  const api = useApi();

  return React.useRef({
    findSources: () => {
      return api.get<never, AxiosResponse<ITopicScoreSourceModel[]>, any>(
        '/admin/topics/scores/sources',
      );
    },
    updateSourceDefaultScore: (model: ITopicScoreSourceModel) => {
      return api.put<ITopicScoreSourceModel, AxiosResponse<ITopicScoreSourceModel>, any>(
        `/admin/topics/scores/sources/${model.id}`,
        model,
      );
    },
    removeSource: (sourceId: number) => {
      return api.delete<never, AxiosResponse<void>, any>(
        `/admin/topics/scores/sources/${sourceId}`,
      );
    },
    findRules: (sourceId: number) => {
      return api.get<never, AxiosResponse<ITopicScoreRuleModel[]>, any>(
        `/admin/topics/scores/sources/${sourceId}/rules`,
      );
    },
    findSections: (sourceId: number) => {
      return api.get<never, AxiosResponse<string[]>, any>(
        `/admin/topics/scores/sources/${sourceId}/sections`,
      );
    },
    reorderRules: (sourceId: number, ruleIds: number[]) => {
      return api.put<number[], AxiosResponse<ITopicScoreRuleModel[]>, any>(
        `/admin/topics/scores/sources/${sourceId}/rules/order`,
        ruleIds,
      );
    },
    addRule: (model: ITopicScoreRuleModel) => {
      return api.post<ITopicScoreRuleModel, AxiosResponse<ITopicScoreRuleModel>, any>(
        '/admin/topics/scores/rules',
        model,
      );
    },
    updateRule: (model: ITopicScoreRuleModel) => {
      return api.put<ITopicScoreRuleModel, AxiosResponse<ITopicScoreRuleModel>, any>(
        `/admin/topics/scores/rules/${model.id}`,
        model,
      );
    },
    deleteRule: (model: ITopicScoreRuleModel) => {
      return api.delete<ITopicScoreRuleModel, AxiosResponse<ITopicScoreRuleModel>, any>(
        `/admin/topics/scores/rules/${model.id}`,
        { data: model },
      );
    },
    test: (model: ITopicScoreTestRequestModel) => {
      return api.post<ITopicScoreTestRequestModel, AxiosResponse<ITopicScoreTestResultModel>, any>(
        '/admin/topics/scores/test',
        model,
      );
    },
    previewRescore: (model: ITopicRescoreRequestModel) => {
      return api.post<ITopicRescoreRequestModel, AxiosResponse<ITopicRescorePreviewModel>, any>(
        '/admin/topics/scores/rescore/preview',
        model,
      );
    },
    rescore: (model: ITopicRescoreRequestModel) => {
      return api.post<ITopicRescoreRequestModel, AxiosResponse<ITopicRescoreJobModel>, any>(
        '/admin/topics/scores/rescore',
        model,
      );
    },
    findRescoreJobs: () => {
      return api.get<never, AxiosResponse<ITopicRescoreJobModel[]>, any>(
        '/admin/topics/scores/rescore',
      );
    },
    findRescoreJob: (id: number) => {
      return api.get<never, AxiosResponse<ITopicRescoreJobModel>, any>(
        `/admin/topics/scores/rescore/${id}`,
      );
    },
  }).current;
};
