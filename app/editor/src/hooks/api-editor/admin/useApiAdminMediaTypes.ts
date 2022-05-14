import React from 'react';
import { defaultEnvelope, ILifecycleToasts, toQueryString } from 'tno-core';

import { IMediaTypeFilter, IMediaTypeModel, IPaged, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminMediaTypes = (
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
      findAllMediaTypes: () => {
        return api.get<IMediaTypeModel[]>(`/admin/media/types/all`);
      },
      findMediaTypes: (filter: IMediaTypeFilter) => {
        return api.get<IPaged<IMediaTypeModel>>(`/admin/media/types?${toQueryString(filter)}`);
      },
      getMediaType: (id: number) => {
        return api.get<IMediaTypeModel>(`/admin/media/types/${id}`);
      },
      addMediaType: (model: IMediaTypeModel) => {
        return api.post<IMediaTypeModel>(`/admin/media/types`, model);
      },
      updateMediaType: (model: IMediaTypeModel) => {
        return api.put<IMediaTypeModel>(`/admin/media/types/${model.id}`, model);
      },
      deleteMediaType: (model: IMediaTypeModel) => {
        return api.delete<IMediaTypeModel>(`/admin/media/types/${model.id}`, { data: model });
      },
    }),
    [api],
  );
};
