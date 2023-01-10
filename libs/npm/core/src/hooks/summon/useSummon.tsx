import axios, { AxiosInstance, AxiosRequestConfig, RawAxiosRequestHeaders } from 'axios';
import { isEmpty } from 'lodash';
import React from 'react';
import { toast } from 'react-toastify';

import { ILifecycleToasts, SummonContext } from '.';

export const defaultEnvelope = (x: any) => ({ data: { records: x } });
let getToken = (): string | null | undefined => {
  throw new Error('Must use useSummon before fetch');
};

/**
 * Global axios instance collection for each unique baseURL.
 */
let axiosInstances = {
  default: {},
};

/**
 * useSummon hook properties.
 */
export interface ISummonProps extends AxiosRequestConfig<any> {
  lifecycleToasts?: ILifecycleToasts;
  selector?: Function;
  envelope?: typeof defaultEnvelope;
}

/**
 * Wrapper for axios to include authentication token and error handling.
 * @param param0 Axios parameters.
 */
export const useSummon = ({
  lifecycleToasts,
  selector,
  envelope = defaultEnvelope,
  baseURL,
  ...rest
}: ISummonProps = {}) => {
  const state = React.useContext(SummonContext);
  let loadingToastId: React.ReactText | undefined = undefined;

  // Need to set this globally so that all useSummon instances have the latest token.
  getToken = () => state.token;
  const instances = React.useRef<any>(axiosInstances);
  const instance = (instances.current as any)[baseURL ?? 'default'] as AxiosInstance;

  if (instance === undefined) {
    const summon = axios.create({
      ...rest,
      baseURL,
      headers: {
        'Access-Control-Allow-Origin': '*',
        ...(rest.headers ?? ({} as any)),
      },
    });

    summon.interceptors.request.use((config) => {
      (config.headers! as RawAxiosRequestHeaders).Authorization = `Bearer ${getToken()}`;
      if (selector !== undefined) {
        const storedValue = selector(state);

        if (!isEmpty(storedValue)) {
          throw new axios.Cancel(JSON.stringify(envelope(storedValue)));
        }
      }
      if (lifecycleToasts?.loadingToast) {
        loadingToastId = lifecycleToasts.loadingToast();
      }
      return config;
    });

    summon.interceptors.response.use(
      (response) => {
        if (lifecycleToasts?.successToast && response.status < 300) {
          loadingToastId && toast.dismiss(loadingToastId);
          lifecycleToasts.successToast();
        } else if (lifecycleToasts?.errorToast && response.status >= 300) {
          lifecycleToasts.errorToast();
        }
        return response;
      },
      (error) => {
        if (axios.isCancel(error)) {
          return Promise.resolve(error.message);
        }
        if (lifecycleToasts?.errorToast) {
          loadingToastId && toast.dismiss(loadingToastId);
          lifecycleToasts.errorToast();
        }

        // TODO: This is not returning the error to an async/await try/catch implementation...
        //const errorMessage =
        //  errorToastMessage || (error.response && error.response.data.message) || String.ERROR;
        return Promise.reject(error);
      },
    );

    (axiosInstances as any)[baseURL ?? 'default'] = summon;
    return summon;
  }

  return instance;
};
