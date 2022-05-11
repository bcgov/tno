import React from 'react';
import { defaultEnvelope, ILifecycleToasts, toQueryString } from 'tno-core';

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

  return React.useMemo(
    () => ({
      findContentReferences: (filter: IContentReferenceFilter) => {
        return api.get<IPaged<IContentReferenceModel>>(
          `/admin/content/references?${toQueryString(filter)}`,
        );
      },
      getContentReference: (source: string, uid: string) => {
        return api.get<IContentReferenceModel>(`/admin/content/references/${source}?uid={uid}`);
      },
      updateContentReference: (model: IContentReferenceModel) => {
        return api.put<IContentReferenceModel>(`/admin/content/references/${model.source}`, model);
      },
      deleteContentReference: (model: IContentReferenceModel) => {
        return api.delete<IContentReferenceModel>(`/admin/content/references/${model.source}`, {
          data: model,
        });
      },
    }),
    [api],
  );
};
