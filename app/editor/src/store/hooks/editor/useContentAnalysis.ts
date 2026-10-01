import { AxiosResponse } from 'axios';
import {
  type IAnalysisJobModel,
  type IContentAnalysisDetailsModel,
} from 'features/content/form/interfaces';
import React from 'react';
import { useApi } from 'tno-core';

import { useAjaxWrapper } from '..';

interface IContentAnalysisController {
  getContentAnalysis: (contentId: number) => Promise<IContentAnalysisDetailsModel>;
  requestAnalysis: (contentId: number) => Promise<IAnalysisJobModel>;
}

/**
 * A content item's Content-Analysis results, and requests to analyze it again.
 * @returns Controller.
 */
export const useContentAnalysis = (): IContentAnalysisController => {
  const api = useApi();
  const dispatch = useAjaxWrapper();

  return React.useMemo(
    () => ({
      getContentAnalysis: async (contentId: number) =>
        (
          await dispatch(
            'get-content-analysis',
            () =>
              api.get<never, AxiosResponse<IContentAnalysisDetailsModel>, any>(
                `/editor/contents/${contentId}/analysis`,
              ),
            undefined,
            true,
          )
        ).data,
      requestAnalysis: async (contentId: number) =>
        (
          await dispatch('request-content-analysis', () =>
            api.post<never, AxiosResponse<IAnalysisJobModel>, any>(
              `/editor/contents/${contentId}/analysis`,
            ),
          )
        ).data,
    }),
    [api, dispatch],
  );
};
