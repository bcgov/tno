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
        onlyThrowError: boolean = false,
      ): Promise<AxiosResponse<T>> => {
        try {
          app.addRequest(name, group, isSilent);
          return await request();
        } catch (error) {
          // TODO: Capture error information.
          const ae = error as AxiosError;

          if (ae.response?.status === 304) throw error;

          let message =
            'An unexpected error has occurred.  If it persists please contact technical support.';
          let detail = '';
          const data = ae.response?.data as any;
          if (ae.response?.status === 401) message = 'Authentication required.';
          else if (ae.response?.status === 403)
            message = 'Authorization required.  Your account does not have access.';
          else if (ae.response?.status === 504)
            message =
              'A system network error has occurred.  If it persists please contact technical support.';
          else if (typeof data === 'string' && !!data) message = data;
          else if (
            data instanceof Blob &&
            data.type &&
            data.type.toLocaleLowerCase().indexOf('json') !== -1
          ) {
            const json = JSON.parse(await data.text());
            message = json.error;
            detail = message !== json.detail ? json.detail : undefined;
          } else if (!!data?.error) {
            if (ae.response?.status === 500) message = `${message} ${data.error}`;
            else message = `${data.error}`;
            detail = message.trim() !== data.details?.trim() ? data.details : undefined;
          } else if (!!data?.errors) {
            message = Object.entries(data.errors)
              .map((p) => p.toString())
              .toString();
          }
          if (!onlyThrowError) {
            app.addError({
              status: ae.response?.status,
              statusText: ae.response?.statusText,
              data: ae.response?.data,
              message: message,
              detail,
            });
          }

          throw error;
        } finally {
          app.removeRequest(name);
        }
      },
    [app],
  );
};
