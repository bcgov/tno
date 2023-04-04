import React from 'react';
import { IFolderModel, IItemModel, useApiStorage } from 'tno-core';

import { useAjaxWrapper } from '..';

interface IStorageController {
  folderExists: (locationId: number, path?: string) => Promise<boolean>;
  getFolder: (locationId: number, path?: string) => Promise<IFolderModel>;
  upload: (
    locationId: number,
    path: string,
    file: File,
    overwrite?: boolean,
    onUploadProgress?: (progressEvent: any) => void,
  ) => Promise<IItemModel>;
  download: (locationId: number, path: string) => Promise<unknown>;
  stream: (locationId: number, path: string) => Promise<unknown>;
  move: (locationId: number, path: string, destination: string) => Promise<IItemModel>;
  delete: (locationId: number, path: string) => Promise<IItemModel>;
  clip: (
    locationId: number,
    path: string,
    start: string,
    end: string,
    outputName: string,
  ) => Promise<IItemModel>;
  join: (locationId: number, path: string, prefix: string) => Promise<IItemModel>;
}

export const useStorage = (): IStorageController => {
  const dispatch = useAjaxWrapper();
  const api = useApiStorage();

  const controller = React.useMemo(
    () => ({
      folderExists: async (locationId?: number, path?: string) => {
        const response = await dispatch<string>('storage-folder-exists', () =>
          api.folderExists(locationId, path),
        );

        return response.status === 200;
      },
      getFolder: async (locationId: number, path?: string) => {
        return (await dispatch<IFolderModel>('get-storage', () => api.getFolder(locationId, path)))
          .data;
      },
      upload: async (
        locationId: number,
        path: string,
        file: File,
        overwrite?: boolean,
        onUploadProgress?: (progressEvent: any) => void,
      ) => {
        return (
          await dispatch<IItemModel>('storage-upload', () =>
            api.upload(locationId, path, file, overwrite, onUploadProgress),
          )
        ).data;
      },
      download: async (locationId: number, path: string) => {
        return (
          await dispatch<IItemModel>('storage-download', () => api.download(locationId, path))
        ).data;
      },
      stream: async (locationId: number, path: string) => {
        return (await dispatch<string>('storage-stream', () => api.stream(locationId, path))).data;
      },
      move: async (locationId: number, path: string, destination: string) => {
        return (
          await dispatch<IItemModel>('storage-move', () => api.move(locationId, path, destination))
        ).data;
      },
      delete: async (locationId: number, path: string) => {
        return (await dispatch<IItemModel>('storage-delete', () => api.delete(locationId, path)))
          .data;
      },
      clip: async (
        locationId: number,
        path: string,
        start: string,
        end: string,
        outputName: string,
      ) => {
        return (
          await dispatch<IItemModel>('storage-clip', () =>
            api.clip(locationId, path, start, end, outputName),
          )
        ).data;
      },
      join: async (locationId: number, path: string, prefix: string) => {
        return (
          await dispatch<IItemModel>('storage-join', () => api.join(locationId, path, prefix))
        ).data;
      },
    }),
    [dispatch, api],
  );

  return controller;
};
