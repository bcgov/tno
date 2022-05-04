import { AxiosError, AxiosResponse } from 'axios';
import { ICacheModel } from 'hooks/api-editor';
import { addOrUpdateArray, getFromLocalStorage } from 'utils';

import { initFromLocalStorage } from '.';

/**
 * Extracts the etag key from local storage.
 * Makes an AJAX request for the content and includes the etag.
 * If API returns 304 it returns the content from local storage.
 * @param key A unique key to identify the cached etag, and the dispatch name.
 * @param dispatch Dispatch function that handles the AJAX request and response.
 * @param fetch AJAX function that makes the request to the API.
 * @param store Redux function to store the results.
 * @returns The results from either the AJAX request or local storage.
 */
export const fetchIfNoneMatch = async <T>(
  key: string,
  dispatch: <T>(
    url: string,
    request: () => Promise<AxiosResponse<T[], any>>,
    response?: ((response: AxiosResponse<T[], any>) => void) | undefined,
  ) => Promise<T[]>,
  fetch: (etag?: string) => Promise<AxiosResponse<T[], any>>,
  store: (results: T[]) => void,
) => {
  return await initFromLocalStorage<T>(key, async (items) => {
    try {
      // If local cache doesn't have items don't include the etag.
      let etag = !!items.length
        ? getFromLocalStorage<ICacheModel[]>('etags', []).find((c) => c.key === key)?.value
        : undefined;

      const result = await dispatch(
        key,
        () => fetch(etag),
        (response) => {
          etag = response.headers['etag'];
        },
      );

      store(result);
      const cache = addOrUpdateArray(
        { key: key, value: etag },
        getFromLocalStorage<ICacheModel[]>('etags', []),
        (c) => c.key === key,
      );
      localStorage.setItem('etags', JSON.stringify(cache));

      return result;
    } catch (error) {
      const ae = error as AxiosError;
      // 304 means the client already has up-to-date results.
      if (ae.response?.status === 304) {
        store(items);
        return items;
      }
      throw error;
    }
  });
};
