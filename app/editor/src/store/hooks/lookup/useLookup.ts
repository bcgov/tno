import {
  IActionModel,
  ICategoryModel,
  IContentTypeModel,
  IDataSourceModel,
  ILicenseModel,
  IMediaTypeModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceMetricModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
  useApiSourceActions,
  useApiSourceMetrics,
} from 'hooks/api-editor';
import {
  useApiActions,
  useApiCategories,
  useApiContentTypes,
  useApiDataSources,
  useApiLicenses,
  useApiMediaTypes,
  useApiSeries,
  useApiTags,
  useApiTonePools,
  useApiUsers,
} from 'hooks/api-editor';
import React from 'react';
import { useLookupStore } from 'store/slices';
import { ILookupState } from 'store/slices/lookup';

import { useApiDispatcher } from '..';
import { initFromLocalStorage } from './utils';

interface ILookupController {
  getActions: (refresh?: boolean) => Promise<IActionModel[]>;
  getSourceActions: (refresh?: boolean) => Promise<ISourceActionModel[]>;
  getSourceMetrics: (refresh?: boolean) => Promise<ISourceMetricModel[]>;
  getCategories: (refresh?: boolean) => Promise<ICategoryModel[]>;
  getContentTypes: (refresh?: boolean) => Promise<IContentTypeModel[]>;
  getLicenses: (refresh?: boolean) => Promise<ILicenseModel[]>;
  getMediaTypes: (refresh?: boolean) => Promise<IMediaTypeModel[]>;
  getDataSources: (refresh?: boolean) => Promise<IDataSourceModel[]>;
  getSeries: (refresh?: boolean) => Promise<ISeriesModel[]>;
  getTags: (refresh?: boolean) => Promise<ITagModel[]>;
  getTonePools: (refresh?: boolean) => Promise<ITonePoolModel[]>;
  getUsers: (refresh?: boolean) => Promise<IUserModel[]>;
  init: (refresh?: boolean) => void;
}

export const useLookup = (): [ILookupState, ILookupController] => {
  const [state, store] = useLookupStore();
  const dispatch = useApiDispatcher();
  const actions = useApiActions();
  const sourceActions = useApiSourceActions();
  const sourceMetrics = useApiSourceMetrics();
  const categories = useApiCategories();
  const contentTypes = useApiContentTypes();
  const licenses = useApiLicenses();
  const mediaTypes = useApiMediaTypes();
  const dataSources = useApiDataSources();
  const series = useApiSeries();
  const tags = useApiTags();
  const tonePools = useApiTonePools();
  const users = useApiUsers();

  // TODO: Handle situation where local storage has stale data.
  const controller = React.useRef({
    getActions: async (refresh: boolean = false) => {
      return await initFromLocalStorage<IActionModel[]>('actions', [], async (items) => {
        const result =
          refresh || !items.length
            ? await dispatch('get-actions', () => actions.getActions())
            : items;
        if (refresh || !!items.length) store.storeActions(result);
        return result;
      });
    },
    getSourceActions: async (refresh: boolean = false) => {
      return await initFromLocalStorage<ISourceActionModel[]>(
        'source-actions',
        [],
        async (items) => {
          const result =
            refresh || !items.length
              ? await dispatch('get-source-actions', () => sourceActions.getActions())
              : items;
          if (refresh || !!items.length) store.storeSourceActions(result);
          return result;
        },
      );
    },
    getSourceMetrics: async (refresh: boolean = false) => {
      return await initFromLocalStorage<ISourceMetricModel[]>(
        'source-metrics',
        [],
        async (items) => {
          const result =
            refresh || !items.length
              ? await dispatch('get-source-metrics', () => sourceMetrics.getMetrics())
              : items;
          if (refresh || !!items.length) store.storeSourceMetrics(result);
          return result;
        },
      );
    },
    getCategories: async (refresh: boolean = false) => {
      return await initFromLocalStorage<ICategoryModel[]>('categories', [], async (items) => {
        const result =
          refresh || !items.length
            ? await dispatch('get-categories', () => categories.getCategories())
            : items;
        if (refresh || !!items.length) store.storeCategories(result);
        return result;
      });
    },
    getContentTypes: async (refresh: boolean = false) => {
      return await initFromLocalStorage<IContentTypeModel[]>('content-types', [], async (items) => {
        const result =
          refresh || !items.length
            ? await dispatch('get-content-types', () => contentTypes.getContentTypes())
            : items;
        if (refresh || !!items.length) store.storeContentTypes(result);
        return result;
      });
    },
    getLicenses: async (refresh: boolean = false) => {
      return await initFromLocalStorage<ILicenseModel[]>('licenses', [], async (items) => {
        const result =
          refresh || !items.length
            ? await dispatch('get-licenses', () => licenses.getLicenses())
            : items;
        if (refresh || !!items.length) store.storeLicenses(result);
        return result;
      });
    },
    getMediaTypes: async (refresh: boolean = false) => {
      return await initFromLocalStorage<IMediaTypeModel[]>('media-types', [], async (items) => {
        const result =
          refresh || !items.length
            ? await dispatch('get-media-types', () => mediaTypes.getMediaTypes())
            : items;
        if (refresh || !!items.length) store.storeMediaTypes(result);
        return result;
      });
    },
    getDataSources: async (refresh: boolean = false) => {
      return await initFromLocalStorage<IDataSourceModel[]>('data-sources', [], async (items) => {
        const result =
          refresh || !items.length
            ? await dispatch('get-data-sources', () => dataSources.getDataSources())
            : items;
        if (refresh || !!items.length) store.storeDataSources(result);
        return result;
      });
    },
    getSeries: async (refresh: boolean = false) => {
      return await initFromLocalStorage<ISeriesModel[]>('series', [], async (items) => {
        const result =
          refresh || !items.length ? await dispatch('get-series', () => series.getSeries()) : items;
        if (refresh || !!items.length) store.storeSeries(result);
        return result;
      });
    },
    getTags: async (refresh: boolean = false) => {
      return await initFromLocalStorage<ITagModel[]>('tags', [], async (items) => {
        const result =
          refresh || !items.length ? await dispatch('get-tags', () => tags.getTags()) : items;
        if (refresh || !!items.length) store.storeTags(result);
        return result;
      });
    },
    getTonePools: async (refresh: boolean = false) => {
      return await initFromLocalStorage<ITonePoolModel[]>('tone-pools', [], async (items) => {
        const result =
          refresh || !items.length
            ? await dispatch('get-tone-pools', () => tonePools.getTonePools())
            : items;
        if (refresh || !!items.length) store.storeTonePools(result);
        return result;
      });
    },
    getUsers: async (refresh: boolean = false) => {
      return await initFromLocalStorage<IUserModel[]>('users', [], async (items) => {
        const result =
          refresh || !items.length ? await dispatch('get-users', () => users.getUsers()) : items;
        if (refresh || !!items.length) store.storeUsers(result);
        return result;
      });
    },
    init: (refresh: boolean = false) => {
      // TODO: Handle failures
      if (!state.actions.length || refresh) controller.getActions();
      if (!state.sourceActions.length || refresh) controller.getSourceActions();
      if (!state.sourceMetrics.length || refresh) controller.getSourceMetrics();
      if (!state.categories.length || refresh) controller.getCategories();
      if (!state.contentTypes.length || refresh) controller.getContentTypes();
      if (!state.licenses.length || refresh) controller.getLicenses();
      if (!state.mediaTypes.length || refresh) controller.getMediaTypes();
      if (!state.dataSources.length || refresh) controller.getDataSources();
      if (!state.series.length || refresh) controller.getSeries();
      if (!state.tags.length || refresh) controller.getTags();
      if (!state.tonePools.length || refresh) controller.getTonePools();
      if (!state.users.length || refresh) controller.getUsers();
    },
  }).current;

  return [state, controller];
};
