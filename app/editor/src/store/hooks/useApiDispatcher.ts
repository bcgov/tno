import { AxiosError, AxiosResponse } from 'axios';
import React from 'react';
import { useAppStore } from 'store/slices';

export interface apiDispatcher<T> {
  (
    /**
     * URL or name to identify this specific request.
     */
    name: string,
    /**
     * Callback that will make the request.
     */
    request: () => Promise<AxiosResponse<T, any>>,
  ): Promise<T>;
}

export const useApiDispatcher = () => {
  const [, app] = useAppStore();

  return React.useMemo(
    () =>
      async <T>(
        name: string,
        request: () => Promise<AxiosResponse<T, any>>,
        response: ((response: AxiosResponse<T, any>) => void) | undefined = undefined,
      ) => {
        try {
          app.addRequest(name);
          const res = await request();
          response?.(res);
          const data = res.data as T;

          return data;
        } catch (error) {
          // TODO: Capture error information.
          const ae = error as AxiosError;

          if (ae.response?.status === 304) throw error;

          let message = 'An unexpected error has occurred.';
          const data = ae.response?.data as any;
          if (typeof data === 'string') message = data;
          else if (data?.error) message = data?.error;
          app.addError({
            status: ae.response?.status,
            statusText: ae.response?.statusText,
            data: ae.response?.data,
            message: message ?? 'An unexpected error has occurred.',
          });
          throw error;
        } finally {
          app.removeRequest(name);
        }
      },
    [app],
  );
};
