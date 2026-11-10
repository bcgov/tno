import { AxiosResponse } from 'axios';
import { IContentAnalysisModel } from 'features/content/view-content/IContentAnalysisModel';
import React from 'react';
import { useApi } from 'tno-core';

import { useAjaxWrapper } from '..';

/** Load existing analysis without granting subscribers editorial actions. */
export const useContentAnalysis = () => {
  const api = useApi();
  const dispatch = useAjaxWrapper();
  return React.useCallback(
    async (contentId: number) => {
      const response = await dispatch(
        'get-content-analysis',
        () =>
          api.get<never, AxiosResponse<IContentAnalysisModel | null>, any>(
            `/subscriber/contents/${contentId}/analysis`,
          ),
        undefined,
        true,
      );
      return response.data;
    },
    [api, dispatch],
  );
};
