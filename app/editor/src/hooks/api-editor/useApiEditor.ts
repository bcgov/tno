import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, LifecycleToasts, useSummon } from 'tno-core';
import { toQueryString } from 'utils';

import {
  IActionModel,
  IContentFilter,
  IContentModel,
  IContentTypeModel,
  IMediaTypeModel,
  IPaged,
  ITagModel,
  ITonePoolModel,
  IUserModel,
  Settings,
} from '.';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiEditor = (
  options: {
    lifecycleToasts?: LifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const summon = useSummon({ ...options, baseURL: options.baseURL ?? Settings.ApiPath });

  const handleRequest = async <T>(request: () => Promise<AxiosResponse<T, T>>) => {
    try {
      const res = await request();
      return res.data as T;
    } catch (error) {
      // TODO: Handle errors.
      console.error(error);
      throw error;
    }
  };

  return React.useMemo(
    () => ({
      getActions: async () => {
        return await handleRequest<IActionModel[]>(() => summon.get(`/editor/actions`));
      },
      getContents: async (pageIndex?: number, pageSize?: number, filter?: IContentFilter) => {
        const params = {
          page: (pageIndex ?? 0) + 1,
          quantity: pageSize,
          ...filter,
          actions: filter?.actions?.length ? filter.actions : undefined,
        };
        return await handleRequest<IPaged<IContentModel>>(() =>
          summon.get(`/editor/contents?${toQueryString(params)}`),
        );
      },
      getContentTypes: async () => {
        return await handleRequest<IContentTypeModel[]>(() => summon.get(`/editor/content/types`));
      },
      getMediaTypes: async () => {
        return await handleRequest<IMediaTypeModel[]>(() => summon.get(`/editor/media/types`));
      },
      getTags: async () => {
        return await handleRequest<ITagModel[]>(() => summon.get(`/editor/tags`));
      },
      getTonePools: async () => {
        return await handleRequest<ITonePoolModel[]>(() => summon.get(`/editor/tone/pools`));
      },
      getUsers: async () => {
        return await handleRequest<IUserModel[]>(() => summon.get(`/editor/users`));
      },
    }),
    [summon],
  );
};

export default useApiEditor;
