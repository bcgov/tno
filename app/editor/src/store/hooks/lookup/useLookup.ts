import {
  IActionModel,
  ICacheModel,
  ICategoryModel,
  IClaimModel,
  IContentTypeModel,
  IDataLocationModel,
  IDataSourceModel,
  ILicenseModel,
  ILookupModel,
  IMediaTypeModel,
  IRoleModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceMetricModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
  useApiActions,
  useApiCache,
  useApiCategories,
  useApiClaims,
  useApiContentTypes,
  useApiDataLocations,
  useApiDataSources,
  useApiLicenses,
  useApiLookups,
  useApiMediaTypes,
  useApiRoles,
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
import { getFromLocalStorage } from 'utils';

import { useApiDispatcher } from '..';
import { fetchIfNoneMatch, saveToLocalStorage } from './utils';

interface ILookupController {
  getCache: () => Promise<ICacheModel[]>;
  getLookups: () => Promise<ILookupModel>;
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
  const lookups = useApiLookups();
  const actions = useApiActions();
  const categories = useApiCategories();
  const claims = useApiClaims();
  const contentTypes = useApiContentTypes();
  const dataLocations = useApiDataLocations();
  const dataSources = useApiDataSources();
  const licenses = useApiLicenses();
  const mediaTypes = useApiMediaTypes();
  const roles = useApiRoles();
  const series = useApiSeries();
  const sourceActions = useApiSourceActions();
  const sourceMetrics = useApiSourceMetrics();
  const tags = useApiTags();
  const tonePools = useApiTonePools();
  const users = useApiUsers();

  const controller = React.useMemo(
    () => ({
      getCache: async () => {
        const result = await dispatch('cache', () => cache.getCache());
        store.storeCache(result);
        return result;
      },
      getLookups: async () => {
        return await fetchIfNoneMatch<ILookupModel>(
          'lookups',
          dispatch,
          (etag) => lookups.getLookups(etag),
          (results) => {
            if (!!results) {
              saveToLocalStorage('actions', results.actions, store.storeActions);
              saveToLocalStorage('categories', results.categories, store.storeCategories);
              saveToLocalStorage('claims', results.claims, store.storeClaims);
              saveToLocalStorage('content_types', results.contentTypes, store.storeContentTypes);
              saveToLocalStorage('data_locations', results.dataLocations, store.storeDataLocations);
              saveToLocalStorage('data_sources', results.dataSources, store.storeDataSources);
              saveToLocalStorage('media_types', results.mediaTypes, store.storeMediaTypes);
              saveToLocalStorage('licenses', results.licenses, store.storeLicenses);
              saveToLocalStorage('roles', results.roles, store.storeRoles);
              saveToLocalStorage('series', results.series, store.storeSeries);
              saveToLocalStorage('source_actions', results.sourceActions, store.storeSourceActions);
              saveToLocalStorage('source_metrics', results.sourceMetrics, store.storeSourceMetrics);
              saveToLocalStorage('tags', results.tags, store.storeTags);
              saveToLocalStorage('tone_pools', results.tonePools, store.storeTonePools);
              saveToLocalStorage('users', results.users, store.storeUsers);
              return results;
            } else {
              const lookups = {
                actions: getFromLocalStorage<IActionModel[]>('actions', []),
                categories: getFromLocalStorage<ICategoryModel[]>('categories', []),
                claims: getFromLocalStorage<IClaimModel[]>('claims', []),
                contentTypes: getFromLocalStorage<IContentTypeModel[]>('content_types', []),
                dataLocations: getFromLocalStorage<IDataLocationModel[]>('data_locations', []),
                dataSources: getFromLocalStorage<IDataSourceModel[]>('data_sources', []),
                mediaTypes: getFromLocalStorage<IMediaTypeModel[]>('media_types', []),
                licenses: getFromLocalStorage<ILicenseModel[]>('licenses', []),
                roles: getFromLocalStorage<IRoleModel[]>('roles', []),
                series: getFromLocalStorage<ISeriesModel[]>('series', []),
                sourceActions: getFromLocalStorage<ISourceActionModel[]>('source_actions', []),
                sourceMetrics: getFromLocalStorage<ISourceMetricModel[]>('source_metrics', []),
                tags: getFromLocalStorage<ITagModel[]>('tags', []),
                tonePools: getFromLocalStorage<ITonePoolModel[]>('tone_pools', []),
                users: getFromLocalStorage<IUserModel[]>('users', []),
              };
              store.storeActions(lookups.actions);
              store.storeCategories(lookups.categories);
              store.storeClaims(lookups.claims);
              store.storeContentTypes(lookups.contentTypes);
              store.storeDataLocations(lookups.dataLocations);
              store.storeDataSources(lookups.dataSources);
              store.storeMediaTypes(lookups.mediaTypes);
              store.storeLicenses(lookups.licenses);
              store.storeRoles(lookups.roles);
              store.storeSeries(lookups.series);
              store.storeSourceActions(lookups.sourceActions);
              store.storeSourceMetrics(lookups.sourceMetrics);
              store.storeTags(lookups.tags);
              store.storeTonePools(lookups.tonePools);
              store.storeUsers(lookups.users);
              return lookups;
            }
          },
          false,
        );
      },
      getActions: async () => {
        return await fetchIfNoneMatch<IActionModel[]>(
          'actions',
          dispatch,
          (etag) => actions.getActions(etag),
          (results) => {
            const values = results ?? [];
            store.storeActions(values);
            return values;
          },
        );
      },
      getCategories: async () => {
        return await fetchIfNoneMatch<ICategoryModel[]>(
          'categories',
          dispatch,
          (etag) => categories.getCategories(etag),
          (results) => {
            const values = results ?? [];
            store.storeCategories(values);
            return values;
          },
        );
      },
      getClaims: async () => {
        return await fetchIfNoneMatch<IClaimModel[]>(
          'claims',
          dispatch,
          (etag) => claims.getClaims(etag),
          (results) => {
            const values = results ?? [];
            store.storeClaims(values);
            return values;
          },
        );
      },
      getContentTypes: async () => {
        return await fetchIfNoneMatch<IContentTypeModel[]>(
          'content_types',
          dispatch,
          (etag) => contentTypes.getContentTypes(etag),
          (results) => {
            const values = results ?? [];
            store.storeContentTypes(values);
            return values;
          },
        );
      },
      getDataLocations: async () => {
        return await fetchIfNoneMatch<IDataLocationModel[]>(
          'data_locations',
          dispatch,
          (etag) => dataLocations.getDataLocations(etag),
          (results) => {
            const values = results ?? [];
            store.storeDataLocations(values);
            return values;
          },
        );
      },
      getDataSources: async () => {
        return await fetchIfNoneMatch<IDataSourceModel[]>(
          'data_sources',
          dispatch,
          (etag) => dataSources.getDataSources(etag),
          (results) => {
            const values = results ?? [];
            store.storeDataSources(values);
            return values;
          },
        );
      },
      getLicenses: async () => {
        return await fetchIfNoneMatch<ILicenseModel[]>(
          'licenses',
          dispatch,
          (etag) => licenses.getLicenses(etag),
          (results) => {
            const values = results ?? [];
            store.storeLicenses(values);
            return values;
          },
        );
      },
      getMediaTypes: async () => {
        return await fetchIfNoneMatch<IMediaTypeModel[]>(
          'media_types',
          dispatch,
          (etag) => mediaTypes.getMediaTypes(etag),
          (results) => {
            const values = results ?? [];
            store.storeMediaTypes(values);
            return values;
          },
        );
      },
      getRoles: async () => {
        return await fetchIfNoneMatch<IRoleModel[]>(
          'roles',
          dispatch,
          (etag) => roles.getRoles(etag),
          (results) => {
            const values = results ?? [];
            store.storeRoles(values);
            return values;
          },
        );
      },
      getSeries: async () => {
        return await fetchIfNoneMatch<ISeriesModel[]>(
          'series',
          dispatch,
          (etag) => series.getSeries(etag),
          (results) => {
            const values = results ?? [];
            store.storeSeries(values);
            return values;
          },
        );
      },
      getSourceActions: async () => {
        return await fetchIfNoneMatch<ISourceActionModel[]>(
          'source_actions',
          dispatch,
          (etag) => sourceActions.getActions(etag),
          (results) => {
            const values = results ?? [];
            store.storeSourceActions(values);
            return values;
          },
        );
      },
      getSourceMetrics: async () => {
        return await fetchIfNoneMatch<ISourceMetricModel[]>(
          'source_metrics',
          dispatch,
          (etag) => sourceMetrics.getMetrics(etag),
          (results) => {
            const values = results ?? [];
            store.storeSourceMetrics(values);
            return values;
          },
        );
      },
      getTags: async () => {
        return await fetchIfNoneMatch<ITagModel[]>(
          'tags',
          dispatch,
          (etag) => tags.getTags(etag),
          (results) => {
            const values = results ?? [];
            store.storeTags(values);
            return values;
          },
        );
      },
      getTonePools: async () => {
        return await fetchIfNoneMatch<ITonePoolModel[]>(
          'tone_pools',
          dispatch,
          (etag) => tonePools.getTonePools(etag),
          (results) => {
            const values = results ?? [];
            store.storeTonePools(values);
            return values;
          },
        );
      },
      getUsers: async () => {
        return await fetchIfNoneMatch<IUserModel[]>(
          'users',
          dispatch,
          (etag) => users.getUsers(etag),
          (results) => {
            const values = results ?? [];
            store.storeUsers(values);
            return values;
          },
        );
      },
      init: async () => {
        // TODO: Handle failures
        await controller.getLookups();
      },
    }),
    [
      actions,
      cache,
      categories,
      claims,
      contentTypes,
      dataLocations,
      dataSources,
      dispatch,
      licenses,
      lookups,
      mediaTypes,
      roles,
      series,
      sourceActions,
      sourceMetrics,
      store,
      tags,
      tonePools,
      users,
    ],
  );

  return [state, controller];
};
