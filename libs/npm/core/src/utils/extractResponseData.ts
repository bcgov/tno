import { AxiosResponse } from 'axios';

/**
 * Extract the body data from AxiosResponse.
 * @param request AxiosRequest object.
 * @returns Response body data as specified type.
 */
export const extractResponseData = async <T>(request: () => Promise<AxiosResponse<T, T>>) => {
  const res = await request();
  return res.data as T;
};
