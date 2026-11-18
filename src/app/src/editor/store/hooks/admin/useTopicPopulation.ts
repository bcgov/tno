import { AxiosResponse } from 'axios';
import React from 'react';

import { type ITopicPopulationSettingsModel } from '@/editor/features/admin/content-analysis/interfaces';
import { useAjaxWrapper } from '@/editor/store/hooks';
import { useApi } from '@/shared';

interface ITopicPopulationController {
  getPopulationSettings: () => Promise<ITopicPopulationSettingsModel>;
  updatePopulationSettings: (
    model: ITopicPopulationSettingsModel,
  ) => Promise<ITopicPopulationSettingsModel>;
}

/**
 * Reads and saves how Content-Analysis assigns topics to content.
 * @returns Controller.
 */
export const useTopicPopulation = (): ITopicPopulationController => {
  const api = useApi();
  const dispatch = useAjaxWrapper();

  return React.useMemo(
    () => ({
      getPopulationSettings: async () =>
        (
          await dispatch('get-topic-population', () =>
            api.get<never, AxiosResponse<ITopicPopulationSettingsModel>, any>(
              '/admin/topics/population',
            ),
          )
        ).data,
      updatePopulationSettings: async (model: ITopicPopulationSettingsModel) =>
        (
          await dispatch('update-topic-population', () =>
            api.put<
              ITopicPopulationSettingsModel,
              AxiosResponse<ITopicPopulationSettingsModel>,
              any
            >('/admin/topics/population', model),
          )
        ).data,
    }),
    [api, dispatch],
  );
};
