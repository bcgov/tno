import { AxiosError, AxiosResponse } from 'axios';
import { useAppStore } from 'store/slices';
import { extractResponseData } from 'tno-core';

export interface apiDispatcher<T> {
  (
    /**
     * URL to identify this specific request.
     */
    url: string,
    /**
     * Callback that will make the request.
     */
    request: () => Promise<AxiosResponse<T, T>>,
  ): Promise<T>;
}

export const useApiDispatcher = () => {
  const [, app] = useAppStore();

  return async <T>(url: string, request: () => Promise<AxiosResponse<T, any>>) => {
    try {
      app.addRequest(url);
      const response = await extractResponseData<T>(request);
      return response;
    } catch (error) {
      // TODO: Capture error information.
      const ae = error as AxiosError;
      let message = 'An unexpected error has occurred.';
      if (typeof ae.response?.data === 'string') message = ae.response?.data;
      else if (ae.response?.data?.error) message = ae.response?.data?.error;
      app.addError({
        status: ae.response?.status,
        statusText: ae.response?.statusText,
        data: ae.response?.data,
        message: message ?? 'An unexpected error has occurred.',
      });
      throw error;
    } finally {
      app.removeRequest(url);
    }
  };
};
