import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IContentReferenceFilter, IContentReferenceModel, IPaged, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminContentReferences = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findContentReferences: (filter: IContentReferenceFilter) => {
      return api.get<never, AxiosResponse<IPaged<IContentReferenceModel>>, any>(
        `/admin/content/references?${toQueryString(filter)}`,
      );
    },
    getContentReference: (source: string, uid: string) => {
      return api.get<never, AxiosResponse<IContentReferenceModel>, any>(
        `/admin/content/references/${source}?uid={uid}`,
      );
    },
    updateContentReference: (model: IContentReferenceModel) => {
      return api.put<IContentReferenceModel, AxiosResponse<IContentReferenceModel>, any>(
        `/admin/content/references/${model.source}`,
        model,
      );
    },
    deleteContentReference: (model: IContentReferenceModel) => {
      return api.delete<IContentReferenceModel, AxiosResponse<IContentReferenceModel>, any>(
        `/admin/content/references/${model.source}`,
        {
          data: model,
        },
      );
    },
    findContentIds: (uid: string) => {
      return api.get<number[], AxiosResponse<number[]>, any>(
        `/admin/content/references/content/ids?uid=${uid}`,
      );
    },
  }).current;
};
