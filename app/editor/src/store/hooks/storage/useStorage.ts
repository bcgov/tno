import { IContentModel, IFolderModel, IItemModel, useApiStorage } from 'hooks/api-editor';
import React from 'react';

import { useApiDispatcher } from '..';

interface IStorageController {
  getFolder: (path?: string, location?: string) => Promise<IFolderModel>;
  upload: (
    path: string,
    file: File,
    overwrite?: boolean,
    location?: string,
    onUploadProgress?: (progressEvent: any) => void,
  ) => Promise<IItemModel>;
  download: (path: string, location?: string) => Promise<unknown>;
  stream: (path: string, location?: string) => Promise<unknown>;
  move: (path: string, destination: string, location?: string) => Promise<IItemModel>;
  delete: (path: string, location?: string) => Promise<IItemModel>;
  clip: (path: string, start: string, end: string, outputName: string) => Promise<IItemModel>;
  join: (path: string, prefix: string) => Promise<IItemModel>;
  attach: (id: number, path: string) => Promise<IContentModel>;
}

export const useStorage = (): IStorageController => {
  const dispatch = useApiDispatcher();
  const api = useApiStorage();

  const controller = React.useMemo(
    () => ({
      getFolder: async (path?: string, location?: string) => {
        return await dispatch<IFolderModel>('get-storage', () => api.getFolder(path, location));
      },
      upload: async (
        path: string,
        file: File,
        overwrite?: boolean,
        location?: string,
        onUploadProgress?: (progressEvent: any) => void,
      ) => {
        return await dispatch<IItemModel>('storage-upload', () =>
          api.upload(path, file, overwrite, location, onUploadProgress),
        );
      },
      download: async (path: string, location?: string) => {
        return await dispatch<IItemModel>('storage-download', () => api.download(path, location));
      },
      stream: async (path: string, location?: string) => {
        return await dispatch<IItemModel>('storage-stream', () => api.stream(path, location));
      },
      move: async (path: string, destination: string, location?: string) => {
        return await dispatch<IItemModel>('storage-move', () =>
          api.move(path, destination, location),
        );
      },
      delete: async (path: string, location?: string) => {
        return await dispatch<IItemModel>('storage-delete', () => api.delete(path, location));
      },
      clip: async (path: string, start: string, end: string, outputName: string) => {
        return await dispatch<IItemModel>('storage-clip', () =>
          api.clip(path, start, end, outputName),
        );
      },
      join: async (path: string, prefix: string) => {
        return await dispatch<IItemModel>('storage-join', () => api.join(path, prefix));
      },
      attach: async (id: number, path: string) => {
        return await dispatch<IContentModel>('storage-attach', () => api.attach(id, path));
      },
    }),
    [dispatch, api],
  );

  return controller;
};
