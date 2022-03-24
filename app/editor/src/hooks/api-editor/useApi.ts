import { AxiosInstance, AxiosRequestConfig } from 'axios';
import { useAppStore } from 'store/slices';
import { defaultEnvelope, ILifecycleToasts, useSummon } from 'tno-core';

import { Settings } from '.';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApi = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const summon = useSummon({ ...options, baseURL: options.baseURL ?? Settings.ApiPath });
  const [, app] = useAppStore();

  const handleRequest = async (url: string = 'NO-URL', request: () => Promise<any>) => {
    try {
      app.addRequest(url);
      const response = await request();
      return response;
    } catch (error) {
      // TODO: Capture error information.
      throw error;
    } finally {
      app.removeRequest(url);
    }
  };

  return {
    request: (config: AxiosRequestConfig<any>) => {
      return handleRequest(config.url, () => summon.request(config));
    },
    get: (url: string, config?: AxiosRequestConfig<any> | undefined) => {
      return handleRequest(url, () => summon.get(url, config));
    },
    post: (url: string, data?: any, config?: AxiosRequestConfig<any> | undefined) => {
      return handleRequest(url, () => summon.post(url, data, config));
    },
    put: (url: string, data?: any, config?: AxiosRequestConfig<any> | undefined) => {
      return handleRequest(url, () => summon.put(url, data, config));
    },
    delete: (url: string, config?: AxiosRequestConfig<any> | undefined) => {
      return handleRequest(url, () => summon.delete(url, config));
    },
  } as AxiosInstance;
};
