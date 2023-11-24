import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IMediaTypeModel, useApi } from '..';

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

  return React.useRef({
    findAllMediaTypes: () => {
      return api.get<never, AxiosResponse<IMediaTypeModel[]>, any>(`/admin/media-types`);
    },
    getMediaType: (id: number) => {
      return api.get<never, AxiosResponse<IMediaTypeModel>, any>(`/admin/media-types/${id}`);
    },
    addMediaType: (model: IMediaTypeModel) => {
      return api.post<IMediaTypeModel, AxiosResponse<IMediaTypeModel>, any>(
        `/admin/media-types`,
        model,
      );
    },
    updateMediaType: (model: IMediaTypeModel) => {
      return api.put<IMediaTypeModel, AxiosResponse<IMediaTypeModel>, any>(
        `/admin/media-types/${model.id}`,
        model,
      );
    },
    deleteMediaType: (model: IMediaTypeModel) => {
      return api.delete<IMediaTypeModel, AxiosResponse<IMediaTypeModel>, any>(
        `/admin/media-types/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
