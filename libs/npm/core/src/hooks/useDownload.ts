import { AxiosInstance, AxiosRequestConfig } from 'axios';
import React from 'react';

/**
 * Download configuration options interface.
 */
export interface IDownloadConfig extends AxiosRequestConfig {
  fileName: string;
}

/**
 * Make an AJAX request to download content from the specified endpoint.
 * @param config - Configuration options to make an AJAX request to download content.
 * @param config.url - The url to the endpoint.
 * @param config.method - The HTTP method to use.
 * @param config.headers - The HTTP request headers to include.  By default it will include the JWT bearer token.
 * @param config.fileName - The file name you want to save the download as.  By default it will use the current date.
 * @param config.data - The body of the request.
 */
export const useDownload = (instance: AxiosInstance) => {
  return React.useCallback(
    async (config: IDownloadConfig) => {
      const options = { ...config, headers: { ...config.headers } };
      const response = await instance.request({
        url: options.url,
        headers: options.headers,
        method: options.method ?? 'get',
        responseType: options.responseType ?? 'blob',
        data: options.data,
      });

      const uri = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = uri;
      link.setAttribute('download', options.fileName ?? new Date().toDateString());
      document.body.appendChild(link);
      link.click();

      return response;
    },
    [instance],
  );
};
