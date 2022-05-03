import { AxiosError, AxiosResponse } from 'axios';
import { useAppStore } from 'store/slices';

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

  return async <T>(
    url: string,
    request: () => Promise<AxiosResponse<T, any>>,
    response: ((response: AxiosResponse<T, any>) => void) | undefined = undefined,
  ) => {
    try {
      app.addRequest(url);
      const res = await request();
      response?.(res);
      const data = res.data as T;
      return data;
    } catch (error) {
      // TODO: Capture error information.
      const ae = error as AxiosError;

      if (ae.response?.status === 304) throw error;

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
