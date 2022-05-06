import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IMediaTypeModel, IPaged, useApi } from '..';

/**
 * Common hook to make requests to the PIMS API.
 * @returns CustomAxios object setup for the PIMS API.
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

  return {
    findAllMediaTypes: () => {
      return api.get<IMediaTypeModel[]>(`/admin/media/types`);
    },
    findMediaTypes: () => {
      return api.get<IPaged<IMediaTypeModel>>(`/admin/media/types/find`);
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
  };
};
