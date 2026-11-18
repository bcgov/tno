import { AxiosResponse } from 'axios';
import React from 'react';

import { IFolderContentModel, useApi } from '..';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiEditorFolders = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getContentInFolder: (id: number, includeCalculatedTopicScore: boolean = false) => {
      return api.get<never, AxiosResponse<IFolderContentModel[]>, any>(
        `/editor/folders/${id}/content?includeCalculatedTopicScore=${includeCalculatedTopicScore}`,
      );
    },
  }).current;
};
