import React from 'react';
import { ILookupState, useLookupStore } from 'store/slices';
import {
  getFromLocalStorage,
  IActionModel,
  ICacheModel,
  IDataLocationModel,
  IHolidayModel,
  IIngestTypeModel,
  ILicenseModel,
  IProductModel,
  IRoleModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceModel,
  ITagModel,
  ITonePoolModel,
  ITopicModel,
  ITopicScoreRuleModel,
  IUserModel,
  useApiActions,
  useApiCache,
  useApiLicenses,
  useApiProducts,
  useApiSeries,
  useApiSourceActions,
  useApiSources,
  useApiTags,
  useApiTonePools,
  useApiTopics,
} from 'tno-core';

import { useAjaxWrapper } from '..';
import { useApiMinisters } from '../subscriber/api/useApiMinisters';
import { IMinisterModel } from '../subscriber/interfaces/IMinisterModel';
import { ILookupModel } from './ILookupModel';
import { useApiLookups } from './useApiLookups';
import { fetchIfNoneMatch, saveToLocalStorage } from './utils';

export interface ILookupController {
  getCache: () => Promise<ICacheModel[]>;
  getLookups: () => Promise<ILookupModel>;
  getActions: (refresh?: boolean) => Promise<IActionModel[]>;
  getSourceActions: (refresh?: boolean) => Promise<ISourceActionModel[]>;
  getTopics: (refresh?: boolean) => Promise<ITopicModel[]>;
  getProducts: (refresh?: boolean) => Promise<IProductModel[]>;
  getLicenses: (refresh?: boolean) => Promise<ILicenseModel[]>;
  getSources: (refresh?: boolean) => Promise<ISourceModel[]>;
  getSeries: (refresh?: boolean) => Promise<ISeriesModel[]>;
  getMinisters: (refresh?: boolean) => Promise<IMinisterModel[]>;
  getTags: (refresh?: boolean) => Promise<ITagModel[]>;
  getTonePools: (refresh?: boolean) => Promise<ITonePoolModel[]>;
  init: (refresh?: boolean) => Promise<void>;
}

export const useLookup = (): [ILookupState, ILookupController] => {
  const [state, store] = useLookupStore();
  const dispatch = useAjaxWrapper();
  const cache = useApiCache();
  const lookups = useApiLookups();
  const actions = useApiActions();
  const topics = useApiTopics();
  const products = useApiProducts();
  const sources = useApiSources();
  const licenses = useApiLicenses();
  const ministers = useApiMinisters();
  const series = useApiSeries();
  const sourceActions = useApiSourceActions();
  const tags = useApiTags();
  const tonePools = useApiTonePools();

  const controller = React.useMemo(
    () => ({
      getCache: async () => {
        const response = await dispatch('cache', () => cache.getCache());
        store.storeCache(response.data);
        return response.data;
      },
      getLookups: async () => {
        return await fetchIfNoneMatch<ILookupModel>(
          'lookups',
          dispatch,
          (etag) => lookups.getLookups(etag),
          (results) => {
            if (!!results) {
              saveToLocalStorage('actions', results.actions, store.storeActions);
              saveToLocalStorage('ministers', results.ministers, store.storeMinisters);
              saveToLocalStorage('topics', results.topics, store.storeTopics);
              saveToLocalStorage('products', results.products, store.storeProducts);
              saveToLocalStorage('sources', results.sources, store.storeSources);
              saveToLocalStorage('licenses', results.licenses, store.storeLicenses);
              saveToLocalStorage('series', results.series, store.storeSeries);
              saveToLocalStorage('contributors', results.contributors, store.storeContributors);
              saveToLocalStorage('source_actions', results.sourceActions, store.storeSourceActions);
              saveToLocalStorage('tags', results.tags, store.storeTags);
              saveToLocalStorage('tone_pools', results.tonePools, store.storeTonePools);
              saveToLocalStorage('holidays', results.holidays, store.storeHolidays);
              return results;
            } else {
              const lookups = {
                actions: getFromLocalStorage<IActionModel[]>('actions', []),
                topics: getFromLocalStorage<ITopicModel[]>('topics', []),
                rules: getFromLocalStorage<ITopicScoreRuleModel[]>('rules', []),
                products: getFromLocalStorage<IProductModel[]>('products', []),
                sources: getFromLocalStorage<ISourceModel[]>('sources', []),
                ingestTypes: getFromLocalStorage<IIngestTypeModel[]>('ingest_types', []),
                licenses: getFromLocalStorage<ILicenseModel[]>('licenses', []),
                roles: getFromLocalStorage<IRoleModel[]>('roles', []),
                series: getFromLocalStorage<ISeriesModel[]>('series', []),
                ministers: getFromLocalStorage<IMinisterModel[]>('ministers', []),
                sourceActions: getFromLocalStorage<ISourceActionModel[]>('source_actions', []),
                tags: getFromLocalStorage<ITagModel[]>('tags', []),
                tonePools: getFromLocalStorage<ITonePoolModel[]>('tone_pools', []),
                users: getFromLocalStorage<IUserModel[]>('users', []),
                dataLocations: getFromLocalStorage<IDataLocationModel[]>('dataLocations', []),
                holidays: getFromLocalStorage<IHolidayModel[]>('holidays', []),
              };
              store.storeActions(lookups.actions);
              store.storeTopics(lookups.topics);
              store.storeProducts(lookups.products);
              store.storeSources(lookups.sources);
              store.storeLicenses(lookups.licenses);
              store.storeSeries(lookups.series);
              store.storeSourceActions(lookups.sourceActions);
              store.storeTags(lookups.tags);
              store.storeTonePools(lookups.tonePools);
              store.storeHolidays(lookups.holidays);
              store.storeMinisters(lookups.ministers);
              return lookups;
            }
          },
          false,
          'lookup',
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
          true,
          'lookup',
        );
      },
      getTopics: async () => {
        return await fetchIfNoneMatch<ITopicModel[]>(
          'topics',
          dispatch,
          (etag) => topics.getTopics(etag),
          (results) => {
            const values = results ?? [];
            store.storeTopics(values);
            return values;
          },
          true,
          'lookup',
        );
      },
      getProducts: async () => {
        return await fetchIfNoneMatch<IProductModel[]>(
          'products',
          dispatch,
          (etag) => products.getProducts(etag),
          (results) => {
            const values = results ?? [];
            store.storeProducts(values);
            return values;
          },
          true,
          'lookup',
        );
      },
      getSources: async () => {
        return await fetchIfNoneMatch<ISourceModel[]>(
          'sources',
          dispatch,
          (etag) => sources.getSources(etag),
          (results) => {
            const values = results ?? [];
            store.storeSources(values);
            return values;
          },
          true,
          'lookup',
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
          true,
          'lookup',
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
          true,
          'lookup',
        );
      },
      getMinisters: async () => {
        return await fetchIfNoneMatch<IMinisterModel[]>(
          'ministers',
          dispatch,
          (etag) => ministers.getMinisters(etag),
          (results) => {
            const values = results ?? [];
            store.storeMinisters(values);
            return values;
          },
          true,
          'lookup',
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
          true,
          'lookup',
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
          true,
          'lookup',
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
          true,
          'lookup',
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
      topics,
      products,
      sources,
      dispatch,
      licenses,
      lookups,
      series,
      sourceActions,
      store,
      tags,
      tonePools,
      ministers,
    ],
  );

  return [state, controller];
};
