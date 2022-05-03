import { AxiosError, AxiosResponse } from 'axios';
import { ICacheModel } from 'hooks/api-editor';

import { addOrUpdate, getFromLocalStorage, initFromLocalStorage } from '.';

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
      let etag = getFromLocalStorage<ICacheModel[]>('cache', []).find((c) => c.key === key)?.value;

      const result = await dispatch(
        key,
        () => fetch(etag),
        (response) => {
          etag = response.headers['etag'];
        },
      );

      store(result);
      const cache = addOrUpdate(
        { key: key, value: etag },
        getFromLocalStorage<ICacheModel[]>('cache', []),
        (c) => c.key === key,
      );
      localStorage.setItem('cache', JSON.stringify(cache));

      return result;
    } catch (error) {
      const ae = error as AxiosError;
      if (ae.response?.status === 304) {
        store(items);
        return items;
      }
      throw error;
    }
  });
};
