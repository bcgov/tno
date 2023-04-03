import { AxiosResponse } from 'axios';
import React from 'react';

import { extractFileName, toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IFolderModel, IItemModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiStorage = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    folderExists: (locationId?: number, path?: string) => {
      const params = {
        path,
      };
      return api.get<string, AxiosResponse<string>, any>(
        `/editor/storage${locationId ? `/${locationId}` : ''}/exists?${toQueryString(params)}`,
      );
    },
    getFolder: (locationId?: number, path?: string) => {
      const params = {
        path,
      };
      return api.get<IFolderModel, AxiosResponse<IFolderModel>, any>(
        `/editor/storage${locationId ? `/${locationId}` : ''}?${toQueryString(params)}`,
      );
    },
    upload: (
      locationId: number,
      path: string,
      file: File,
      overwrite?: boolean,
      onUploadProgress?: (progressEvent: any) => void,
    ) => {
      const params = {
        path,
        overwrite,
      };
      const formData = new FormData();
      formData.append('files', file, file.name);
      return api.post<IItemModel, AxiosResponse<IItemModel>, any>(
        `/editor/storage${locationId ? `/${locationId}` : ''}/upload?${toQueryString(params)}`,
        formData,
        { onUploadProgress },
      );
    },
    stream: async (
      locationId: number,
      path: string,
      onUploadProgress?: (progressEvent: any) => void,
    ) => {
      const params = {
        path,
      };
      var response = await api.get<any, AxiosResponse<any>, any>(
        `/editor/storage${locationId ? `/${locationId}` : ''}/stream?${toQueryString(params)}`,
        {
          responseType: 'stream',
          headers: { accept: '*.*' },
          onUploadProgress,
        },
      );

      const uri = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = uri;
      link.setAttribute('download', new Date().toDateString());
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      return response;
    },
    stream2: async (locationId: number, path: string) => {
      const params = { path };
      return await api.get<any, AxiosResponse<any>, any>(
        `/editor/storage${locationId ? `/${locationId}` : ''}/stream?${toQueryString(params)}`,
      );
    },
    download: async (locationId: number, path: string, fileName?: string) => {
      const params = {
        path,
      };
      var response = await api.get<any, AxiosResponse<any>, any>(
        `/editor/storage${locationId ? `/${locationId}` : ''}/download?${toQueryString(params)}`,
        {
          responseType: 'blob',
          headers: { accept: '*.*' },
        },
      );

      const uri = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = uri;
      link.setAttribute(
        'download',
        fileName ?? extractFileName(response.headers) ?? new Date().toDateString(),
      );
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      return response;
    },
    move: (locationId: number, path: string, destination: string) => {
      const params = {
        path,
        destination,
      };
      return api.put<IItemModel, AxiosResponse<IItemModel>, any>(
        `/editor/storage${locationId ? `/${locationId}` : ''}/move?${toQueryString(params)}`,
      );
    },
    delete: (locationId: number, path: string) => {
      const params = {
        path,
      };
      return api.delete<IItemModel, AxiosResponse<IItemModel>, any>(
        `/editor/storage${locationId ? `/${locationId}` : ''}?${toQueryString(params)}`,
      );
    },
    clip: (locationId: number, path: string, start: string, end: string, outputName: string) => {
      const params = {
        path,
        start,
        end,
        outputName,
      };
      return api.post<IItemModel, AxiosResponse<IItemModel>, any>(
        `/editor/storage${locationId ? `/${locationId}` : ''}/clip?${toQueryString(params)}`,
      );
    },
    join: (locationId: number, path: string, prefix: string) => {
      const params = {
        path,
        prefix,
      };
      return api.post<IItemModel, AxiosResponse<IItemModel>, any>(
        `/editor/storage${locationId ? `/${locationId}` : ''}/join?${toQueryString(params)}`,
      );
    },
  }).current;
};
