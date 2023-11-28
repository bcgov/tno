import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { ITopicScoreRuleModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminTopicScoreRules = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllTopicScoreRules: () => {
      return api.get<never, AxiosResponse<ITopicScoreRuleModel[]>, any>(
        `/admin/topics/scores/rules/all`,
      );
    },
    getTopicScoreRule: (id: number) => {
      return api.get<never, AxiosResponse<ITopicScoreRuleModel>, any>(
        `/admin/topics/scores/rules/${id}`,
      );
    },
    addTopicScoreRule: (model: ITopicScoreRuleModel) => {
      return api.post<ITopicScoreRuleModel, AxiosResponse<ITopicScoreRuleModel>, any>(
        `/admin/topics/scores/rules`,
        model,
      );
    },
    updateTopicScoreRule: (model: ITopicScoreRuleModel) => {
      return api.put<ITopicScoreRuleModel, AxiosResponse<ITopicScoreRuleModel>, any>(
        `/admin/topics/scores/rules/${model.id}`,
        model,
      );
    },
    updateTopicScoreRules: (models: ITopicScoreRuleModel[]) => {
      return api.put<ITopicScoreRuleModel[], AxiosResponse<ITopicScoreRuleModel[]>, any>(
        `/admin/topics/scores/rules`,
        models,
      );
    },
    deleteTopicScoreRule: (model: ITopicScoreRuleModel) => {
      return api.delete<ITopicScoreRuleModel, AxiosResponse<ITopicScoreRuleModel>, any>(
        `/admin/topics/scores/rules/${model.id}`,
        {
          data: model,
        },
      );
    },
  }).current;
};
