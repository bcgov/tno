import { AxiosError, AxiosResponse } from 'axios';
import React from 'react';
import { useAppStore } from 'store/slices';

/**
 * Hook provides a common way to make AJAX requests.
 * All requests are added to a a log.
 * All errors are added to the error log.
 * When a request completes it is removed from the log.
 * @returns Function to dispatch AJAX requests.
 */
export const useAjaxWrapper = () => {
  const [, app] = useAppStore();

  return React.useMemo(
    () =>
      async <T>(
        name: string,
        request: () => Promise<AxiosResponse<T>>,
        group: string | string[] | undefined = undefined,
        isSilent: boolean = false,
      ): Promise<AxiosResponse<T>> => {
        try {
          app.addRequest(name, group, isSilent);
          return await request();
        } catch (error) {
          // TODO: Capture error information.
          const ae = error as AxiosError;

          if (ae.response?.status === 304) throw error;

          let message = 'An unexpected error has occurred.';
          let detail = '';
          const data = ae.response?.data as any;

          if (ae.response?.status === 401) message = 'Authentication required.';
          else if (ae.response?.status === 403)
            message = 'Authorization required.  Your account does not have access.';
          else if (typeof data === 'string' && !!data) message = data;
          else if (!!data?.error) {
            message = `${data?.error}`;
            detail = data?.details;
          } else if (!!data?.errors) {
            message = Object.entries(data.errors)
              .map((p) => p.toString())
              .toString();
          }
          app.addError({
            status: ae.response?.status,
            statusText: ae.response?.statusText,
            data: ae.response?.data,
            message: message,
            detail,
          });

          throw error;
        } finally {
          app.removeRequest(name);
        }
      },
    [app],
  );
};
