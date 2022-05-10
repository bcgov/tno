import {
  IActionModel,
  ICacheModel,
  ICategoryModel,
  IContentTypeModel,
  IDataLocationModel,
  IDataSourceModel,
  ILicenseModel,
  IMediaTypeModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceMetricModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
  useApiActions,
  useApiCache,
  useApiCategories,
  useApiContentTypes,
  useApiDataLocations,
  useApiDataSources,
  useApiLicenses,
  useApiMediaTypes,
  useApiSeries,
  useApiSourceActions,
  useApiSourceMetrics,
  useApiTags,
  useApiTonePools,
  useApiUsers,
} from 'hooks/api-editor';
import React from 'react';
import { useLookupStore } from 'store/slices';
import { ILookupState } from 'store/slices/lookup';

import { useApiDispatcher } from '..';
import { fetchIfNoneMatch } from './utils';

interface ILookupController {
  getCache: () => Promise<ICacheModel[]>;
  getActions: (refresh?: boolean) => Promise<IActionModel[]>;
  getDataLocations: (refresh?: boolean) => Promise<IDataLocationModel[]>;
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
  init: (refresh?: boolean) => Promise<void>;
}

export const useLookup = (): [ILookupState, ILookupController] => {
  const [state, store] = useLookupStore();
  const dispatch = useApiDispatcher();
  const cache = useApiCache();
  const actions = useApiActions();
  const dataLocations = useApiDataLocations();
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
  const controller = React.useMemo(
    () => ({
      getCache: async () => {
        const result = await dispatch('cache', () => cache.getCache());
        store.storeCache(result);
        return result;
      },
      getActions: async () => {
        return await fetchIfNoneMatch<IActionModel>(
          'actions',
          dispatch,
          (etag) => actions.getActions(etag),
          (results) => store.storeActions(results),
        );
      },
      getDataLocations: async () => {
        return await fetchIfNoneMatch<IDataLocationModel>(
          'data_locations',
          dispatch,
          (etag) => dataLocations.getDataLocations(etag),
          (results) => store.storeDataLocations(results),
        );
      },
      getSourceActions: async () => {
        return await fetchIfNoneMatch<ISourceActionModel>(
          'source_actions',
          dispatch,
          (etag) => sourceActions.getActions(etag),
          (results) => store.storeSourceActions(results),
        );
      },
      getSourceMetrics: async () => {
        return await fetchIfNoneMatch<ISourceMetricModel>(
          'source_metrics',
          dispatch,
          (etag) => sourceMetrics.getMetrics(etag),
          (results) => store.storeSourceMetrics(results),
        );
      },
      getCategories: async () => {
        return await fetchIfNoneMatch<ICategoryModel>(
          'categories',
          dispatch,
          (etag) => categories.getCategories(etag),
          (results) => store.storeCategories(results),
        );
      },
      getContentTypes: async () => {
        return await fetchIfNoneMatch<IContentTypeModel>(
          'content_types',
          dispatch,
          (etag) => contentTypes.getContentTypes(etag),
          (results) => store.storeContentTypes(results),
        );
      },
      getLicenses: async () => {
        return await fetchIfNoneMatch<ILicenseModel>(
          'licenses',
          dispatch,
          (etag) => licenses.getLicenses(etag),
          (results) => store.storeLicenses(results),
        );
      },
      getMediaTypes: async () => {
        return await fetchIfNoneMatch<IMediaTypeModel>(
          'media_types',
          dispatch,
          (etag) => mediaTypes.getMediaTypes(etag),
          (results) => store.storeMediaTypes(results),
        );
      },
      getDataSources: async () => {
        return await fetchIfNoneMatch<IDataSourceModel>(
          'data_sources',
          dispatch,
          (etag) => dataSources.getDataSources(etag),
          (results) => store.storeDataSources(results),
        );
      },
      getSeries: async () => {
        return await fetchIfNoneMatch<ISeriesModel>(
          'series',
          dispatch,
          (etag) => series.getSeries(etag),
          (results) => store.storeSeries(results),
        );
      },
      getTags: async () => {
        return await fetchIfNoneMatch<ITagModel>(
          'tags',
          dispatch,
          (etag) => tags.getTags(etag),
          (results) => store.storeTags(results),
        );
      },
      getTonePools: async () => {
        return await fetchIfNoneMatch<ITonePoolModel>(
          'tone_pools',
          dispatch,
          (etag) => tonePools.getTonePools(etag),
          (results) => store.storeTonePools(results),
        );
      },
      getUsers: async () => {
        return await fetchIfNoneMatch<IUserModel>(
          'users',
          dispatch,
          (etag) => users.getUsers(etag),
          (results) => store.storeUsers(results),
        );
      },
      init: async () => {
        // TODO: Handle failures
        if (!state.actions.length) await controller.getActions();
        if (!state.dataLocations.length) await controller.getDataLocations();
        if (!state.sourceActions.length) await controller.getSourceActions();
        if (!state.sourceMetrics.length) await controller.getSourceMetrics();
        if (!state.categories.length) await controller.getCategories();
        if (!state.contentTypes.length) await controller.getContentTypes();
        if (!state.licenses.length) await controller.getLicenses();
        if (!state.mediaTypes.length) await controller.getMediaTypes();
        if (!state.dataSources.length) await controller.getDataSources();
        if (!state.series.length) await controller.getSeries();
        if (!state.tags.length) await controller.getTags();
        if (!state.tonePools.length) await controller.getTonePools();
        if (!state.users.length) await controller.getUsers();
      },
    }),
    [
      actions,
      cache,
      categories,
      contentTypes,
      dataLocations,
      dataSources,
      dispatch,
      licenses,
      mediaTypes,
      series,
      sourceActions,
      sourceMetrics,
      state.actions.length,
      state.categories.length,
      state.contentTypes.length,
      state.dataLocations.length,
      state.dataSources.length,
      state.licenses.length,
      state.mediaTypes.length,
      state.series.length,
      state.sourceActions.length,
      state.sourceMetrics.length,
      state.tags.length,
      state.tonePools.length,
      state.users.length,
      store,
      tags,
      tonePools,
      users,
    ],
  );

  return [state, controller];
};
