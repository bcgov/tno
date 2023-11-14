import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IMediaTypeModel, useApiAdminMediaTypes } from 'tno-core';

interface IMediaTypeController {
  findAllMediaTypes: () => Promise<IMediaTypeModel[]>;
  getMediaType: (id: number) => Promise<IMediaTypeModel>;
  addMediaType: (model: IMediaTypeModel) => Promise<IMediaTypeModel>;
  updateMediaType: (model: IMediaTypeModel) => Promise<IMediaTypeModel>;
  deleteMediaType: (model: IMediaTypeModel) => Promise<IMediaTypeModel>;
}

export const useMediaTypes = (): [IAdminState, IMediaTypeController] => {
  const api = useApiAdminMediaTypes();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllMediaTypes: async () => {
        const response = await dispatch<IMediaTypeModel[]>('find-all-media-types', () =>
          api.findAllMediaTypes(),
        );
        store.storeMediaTypes(response.data);
        return response.data;
      },
      getMediaType: async (id: number) => {
        const response = await dispatch<IMediaTypeModel>('get-media-type', () =>
          api.getMediaType(id),
        );
        store.storeMediaTypes((mediaTypes) =>
          mediaTypes.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addMediaType: async (model: IMediaTypeModel) => {
        const response = await dispatch<IMediaTypeModel>('add-media-type', () =>
          api.addMediaType(model),
        );
        store.storeMediaTypes((mediaTypes) => {
          return [...mediaTypes, response.data].sort((a, b) =>
            a.name < b.name ? -1 : a.name > b.name ? 1 : 0,
          );
        });
        await lookup.getLookups();
        return response.data;
      },
      updateMediaType: async (model: IMediaTypeModel) => {
        const response = await dispatch<IMediaTypeModel>('update-media-type', () =>
          api.updateMediaType(model),
        );
        store.storeMediaTypes((mediaTypes) =>
          mediaTypes.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteMediaType: async (model: IMediaTypeModel) => {
        const response = await dispatch<IMediaTypeModel>('delete-media-type', () =>
          api.deleteMediaType(model),
        );
        store.storeMediaTypes((mediaTypes) =>
          mediaTypes.filter((ds) => ds.id !== response.data.id),
        );
        await lookup.getLookups();
        return response.data;
      },
    }),
    [dispatch, store, api, lookup],
  );

  return [state, controller];
};
