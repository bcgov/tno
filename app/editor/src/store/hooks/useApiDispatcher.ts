import { AxiosError, AxiosResponse } from 'axios';
import React from 'react';
import { useAppStore } from 'store/slices';

export const useApiDispatcher = () => {
  const [, app] = useAppStore();

  return React.useMemo(
    () =>
      async <T>(
        name: string,
        request: () => Promise<AxiosResponse<T>>,
      ): Promise<AxiosResponse<T>> => {
        try {
          app.addRequest(name);
          return await request();
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
            message: message,
          });

          throw error;
        } finally {
          app.removeRequest(name);
        }
      },
    [app],
  );
};
