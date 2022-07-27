import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, toQueryString } from 'tno-core';
import { extractFileName } from 'utils';

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
    getFolder: (path?: string, location?: string) => {
      const params = {
        path,
      };
      return api.get<IFolderModel, AxiosResponse<IFolderModel, never>, any>(
        `/editor/storage${location ? `/${location}` : ''}?${toQueryString(params)}`,
      );
    },
    upload: (
      path: string,
      file: File,
      overwrite?: boolean,
      location?: string,
      onUploadProgress?: (progressEvent: any) => void,
    ) => {
      const params = {
        path,
        overwrite,
      };
      const formData = new FormData();
      formData.append('files', file, file.name);
      return api.post<IItemModel, AxiosResponse<IItemModel, never>, any>(
        `/editor/storage${location ? `/${location}` : ''}/upload?${toQueryString(params)}`,
        formData,
        { onUploadProgress },
      );
    },
    stream: async (
      path: string,
      location?: string,
      onUploadProgress?: (progressEvent: any) => void,
    ) => {
      const params = {
        path,
      };
      var response = await api.get<any, AxiosResponse<any, never>, any>(
        `/editor/storage${location ? `/${location}` : ''}/stream?${toQueryString(params)}`,
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
    download: async (path: string, fileName?: string, location?: string) => {
      const params = {
        path,
      };
      var response = await api.get<any, AxiosResponse<any, never>, any>(
        `/editor/storage${location ? `/${location}` : ''}/download?${toQueryString(params)}`,
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
    move: (path: string, destination: string, location?: string) => {
      const params = {
        path,
        destination,
      };
      return api.put<IItemModel, AxiosResponse<IItemModel, never>, any>(
        `/editor/storage${location ? `/${location}` : ''}/move?${toQueryString(params)}`,
      );
    },
    delete: (path: string, location?: string) => {
      const params = {
        path,
      };
      return api.delete<IItemModel, AxiosResponse<IItemModel, never>, any>(
        `/editor/storage${location ? `/${location}` : ''}?${toQueryString(params)}`,
      );
    },
    clip: (
      fileName: string,
      directory: string,
      start: string,
      end: string,
      clipNbr: number,
      prefix: string,
    ) => {
      const params = {
        fileName,
        directory,
        start,
        end,
        clipNbr,
        prefix,
      };
      return api.get<IItemModel, AxiosResponse<IFolderModel, never>, any>(
        `/editor/storage/clip?${toQueryString(params)}`,
      );
    },
    join: (fileName: string, directory: string, prefix: string) => {
      const params = {
        fileName,
        directory,
        prefix,
      };
      return api.put<IFolderModel, AxiosResponse<IFolderModel, never>, any>(
        `/editor/storage/join?${toQueryString(params)}`,
      );
    },
    attach: (id: number, path: string) => {
      const params = {
        path,
      };
      return api.put<IFolderModel, AxiosResponse<IFolderModel, never>, any>(
        `/editor/contents/${id}/attach?${toQueryString(params)}`,
      );
    },
  }).current;
};
