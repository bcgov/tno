import { IMediaTypeModel, IPaged, useApiAdminMediaTypes } from 'hooks/api-editor';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface IMediaTypeController {
  findAllMediaTypes: () => Promise<IMediaTypeModel[]>;
  findMediaTypes: () => Promise<IPaged<IMediaTypeModel>>;
  getMediaType: (id: number) => Promise<IMediaTypeModel>;
  addMediaType: (model: IMediaTypeModel) => Promise<IMediaTypeModel>;
  updateMediaType: (model: IMediaTypeModel) => Promise<IMediaTypeModel>;
  deleteMediaType: (model: IMediaTypeModel) => Promise<IMediaTypeModel>;
}

export const useMediaTypes = (): [IAdminState, IMediaTypeController] => {
  const api = useApiAdminMediaTypes();
  const dispatch = useApiDispatcher();
  const [state, store] = useAdminStore();

  const getMediaTypes = () => state.mediaTypes;

  const controller = React.useMemo(
    () => ({
      findAllMediaTypes: async () => {
        const result = await dispatch<IMediaTypeModel[]>('find-all-media-types', () =>
          api.findAllMediaTypes(),
        );
        store.storeMediaTypes(result);
        return result;
      },
      findMediaTypes: async () => {
        const result = await dispatch<IPaged<IMediaTypeModel>>('find-media-types', () =>
          api.findMediaTypes(),
        );
        return result;
      },
      getMediaType: async (id: number) => {
        const result = await dispatch<IMediaTypeModel>('get-media-type', () =>
          api.getMediaType(id),
        );
        store.storeMediaTypes(
          getMediaTypes().map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addMediaType: async (model: IMediaTypeModel) => {
        const result = await dispatch<IMediaTypeModel>('add-data-source', () =>
          api.addMediaType(model),
        );
        store.storeMediaTypes([...getMediaTypes(), result]);
        return result;
      },
      updateMediaType: async (model: IMediaTypeModel) => {
        const result = await dispatch<IMediaTypeModel>('update-data-source', () =>
          api.updateMediaType(model),
        );
        store.storeMediaTypes(
          getMediaTypes().map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteMediaType: async (model: IMediaTypeModel) => {
        const result = await dispatch<IMediaTypeModel>('delete-data-source', () =>
          api.deleteMediaType(model),
        );
        store.storeMediaTypes(getMediaTypes().filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    [api, dispatch, getMediaTypes, store],
  );

  return [state, controller];
};
