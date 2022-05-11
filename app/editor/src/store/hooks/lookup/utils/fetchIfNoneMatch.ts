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
 * @param saveResults Whether to save the results of the action to local storage.
 * @returns The results from either the AJAX request or local storage.
 */
export const fetchIfNoneMatch = async <T>(
  key: string,
  dispatch: <T>(
    url: string,
    request: () => Promise<AxiosResponse<T, any>>,
    response?: ((response: AxiosResponse<T, any>) => void) | undefined,
  ) => Promise<T>,
  fetch: (etag?: string) => Promise<AxiosResponse<T, any>>,
  store: (results?: T) => T,
  saveResults: boolean = true,
) => {
  return await initFromLocalStorage<T>(
    key,
    async (cacheItems) => {
      try {
        // If local cache doesn't have items don't include the etag.
        let etag =
          !!cacheItems || !saveResults
            ? getFromLocalStorage<ICacheModel[]>('etags', []).find((c) => c.key === key)?.value
            : undefined;

        console.debug(key, etag, cacheItems, saveResults);

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
          return store(cacheItems);
        }
        throw error;
      }
    },
    saveResults,
  );
};
