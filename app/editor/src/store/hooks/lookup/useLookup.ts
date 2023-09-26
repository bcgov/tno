import React from 'react';
import { useLookupStore } from 'store/slices';
import { ILookupState } from 'store/slices/lookup';
import {
  fetchIfNoneMatch,
  getFromLocalStorage,
  IActionModel,
  ICacheModel,
  IContributorModel,
  IDataLocationModel,
  IHolidayModel,
  IIngestTypeModel,
  ILicenseModel,
  ILookupModel,
  IMetricModel,
  IMinisterModel,
  IProductModel,
  IRoleModel,
  ISeriesModel,
  ISettingModel,
  ISourceActionModel,
  ISourceModel,
  ITagModel,
  ITonePoolModel,
  ITopicModel,
  ITopicScoreRuleModel,
  IUserModel,
  saveToLocalStorage,
  useApiEditorActions,
  useApiEditorCache,
  useApiEditorContributors,
  useApiEditorDataLocations,
  useApiEditorIngestTypes,
  useApiEditorLicenses,
  useApiEditorLookups,
  useApiEditorMetrics,
  useApiEditorMinisters,
  useApiEditorProducts,
  useApiEditorRoles,
  useApiEditorSeries,
  useApiEditorSettings,
  useApiEditorSourceActions,
  useApiEditorSources,
  useApiEditorTags,
  useApiEditorTonePools,
  useApiEditorTopics,
  useApiEditorTopicScoreRules,
  useApiEditorUsers,
} from 'tno-core';

import { useAjaxWrapper } from '..';

export interface ILookupController {
  getCache: () => Promise<ICacheModel[]>;
  getLookups: () => Promise<ILookupModel>;
  getActions: (refresh?: boolean) => Promise<IActionModel[]>;
  getSourceActions: (refresh?: boolean) => Promise<ISourceActionModel[]>;
  getMetrics: (refresh?: boolean) => Promise<IMetricModel[]>;
  getTopics: (refresh?: boolean) => Promise<ITopicModel[]>;
  getTopicScoreRules: (refresh?: boolean) => Promise<ITopicScoreRuleModel[]>;
  getProducts: (refresh?: boolean) => Promise<IProductModel[]>;
  getLicenses: (refresh?: boolean) => Promise<ILicenseModel[]>;
  getIngestTypes: (refresh?: boolean) => Promise<IIngestTypeModel[]>;
  getSources: (refresh?: boolean) => Promise<ISourceModel[]>;
  getSeries: (refresh?: boolean) => Promise<ISeriesModel[]>;
  getContributors: (refresh?: boolean) => Promise<IContributorModel[]>;
  getTags: (refresh?: boolean) => Promise<ITagModel[]>;
  getTonePools: (refresh?: boolean) => Promise<ITonePoolModel[]>;
  getUsers: (refresh?: boolean) => Promise<IUserModel[]>;
  getDataLocations: (refresh?: boolean) => Promise<IDataLocationModel[]>;
  getSettings: (refresh?: boolean) => Promise<ISettingModel[]>;
  getMinisters: (refresh?: boolean) => Promise<IMinisterModel[]>;
  init: (refresh?: boolean) => Promise<void>;
}

export const useLookup = (): [ILookupState, ILookupController] => {
  const [state, store] = useLookupStore();
  const dispatch = useAjaxWrapper();
  const cache = useApiEditorCache();
  const lookups = useApiEditorLookups();
  const actions = useApiEditorActions();
  const topics = useApiEditorTopics();
  const rules = useApiEditorTopicScoreRules();
  const products = useApiEditorProducts();
  const sources = useApiEditorSources();
  const licenses = useApiEditorLicenses();
  const ingestTypes = useApiEditorIngestTypes();
  const roles = useApiEditorRoles();
  const series = useApiEditorSeries();
  const contributors = useApiEditorContributors();
  const sourceActions = useApiEditorSourceActions();
  const sourceMetrics = useApiEditorMetrics();
  const tags = useApiEditorTags();
  const tonePools = useApiEditorTonePools();
  const users = useApiEditorUsers();
  const dataLocations = useApiEditorDataLocations();
  const settings = useApiEditorSettings();
  const ministers = useApiEditorMinisters();

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
              saveToLocalStorage('topics', results.topics, store.storeTopics);
              saveToLocalStorage('rules', results.rules, store.storeTopicScoreRules);
              saveToLocalStorage('products', results.products, store.storeProducts);
              saveToLocalStorage('sources', results.sources, store.storeSources);
              saveToLocalStorage('ingest_types', results.ingestTypes, store.storeIngestTypes);
              saveToLocalStorage('licenses', results.licenses, store.storeLicenses);
              saveToLocalStorage('roles', results.roles, store.storeRoles);
              saveToLocalStorage('series', results.series, store.storeSeries);
              saveToLocalStorage('contributors', results.contributors, store.storeContributors);
              saveToLocalStorage('source_actions', results.sourceActions, store.storeSourceActions);
              saveToLocalStorage('metrics', results.metrics, store.storeMetrics);
              saveToLocalStorage('tags', results.tags, store.storeTags);
              saveToLocalStorage('tone_pools', results.tonePools, store.storeTonePools);
              saveToLocalStorage('users', results.users, store.storeUsers);
              saveToLocalStorage('dataLocations', results.dataLocations, store.storeDataLocations);
              saveToLocalStorage('settings', results.settings, store.storeSettings);
              saveToLocalStorage('holidays', results.holidays, store.storeHolidays);
              saveToLocalStorage('ministers', results.ministers, store.storeMinisters);
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
                contributors: getFromLocalStorage<IContributorModel[]>('contributors', []),
                sourceActions: getFromLocalStorage<ISourceActionModel[]>('source_actions', []),
                metrics: getFromLocalStorage<IMetricModel[]>('metrics', []),
                tags: getFromLocalStorage<ITagModel[]>('tags', []),
                tonePools: getFromLocalStorage<ITonePoolModel[]>('tone_pools', []),
                users: getFromLocalStorage<IUserModel[]>('users', []),
                dataLocations: getFromLocalStorage<IDataLocationModel[]>('dataLocations', []),
                settings: getFromLocalStorage<ISettingModel[]>('settings', []),
                holidays: getFromLocalStorage<IHolidayModel[]>('holidays', []),
                ministers: getFromLocalStorage<IMinisterModel[]>('ministers', []),
              };
              store.storeActions(lookups.actions);
              store.storeTopics(lookups.topics);
              store.storeTopicScoreRules(lookups.rules);
              store.storeProducts(lookups.products);
              store.storeSources(lookups.sources);
              store.storeIngestTypes(lookups.ingestTypes);
              store.storeLicenses(lookups.licenses);
              store.storeRoles(lookups.roles);
              store.storeSeries(lookups.series);
              store.storeContributors(lookups.contributors);
              store.storeSourceActions(lookups.sourceActions);
              store.storeMetrics(lookups.metrics);
              store.storeTags(lookups.tags);
              store.storeTonePools(lookups.tonePools);
              store.storeUsers(lookups.users);
              store.storeDataLocations(lookups.dataLocations);
              store.storeSettings(lookups.settings);
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
      getTopicScoreRules: async () => {
        return await fetchIfNoneMatch<ITopicScoreRuleModel[]>(
          'rules',
          dispatch,
          (etag) => rules.getTopicScoreRules(etag),
          (results) => {
            const values = results ?? [];
            store.storeTopicScoreRules(values);
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
      getIngestTypes: async () => {
        return await fetchIfNoneMatch<IIngestTypeModel[]>(
          'ingest_types',
          dispatch,
          (etag) => ingestTypes.getIngestTypes(etag),
          (results) => {
            const values = results ?? [];
            store.storeIngestTypes(values);
            return values;
          },
          true,
          'lookup',
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
      getContributors: async () => {
        return await fetchIfNoneMatch<IContributorModel[]>(
          'contributors',
          dispatch,
          (etag) => contributors.getContributors(etag),
          (results) => {
            const values = results ?? [];
            store.storeContributors(values);
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
      getMetrics: async () => {
        return await fetchIfNoneMatch<IMetricModel[]>(
          'metrics',
          dispatch,
          (etag) => sourceMetrics.getMetrics(etag),
          (results) => {
            const values = results ?? [];
            store.storeMetrics(values);
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
          true,
          'lookup',
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
          true,
          'lookup',
        );
      },
      getSettings: async () => {
        return await fetchIfNoneMatch<ISettingModel[]>(
          'settings',
          dispatch,
          (etag) => settings.getSettings(etag),
          (results) => {
            const values = results ?? [];
            store.storeSettings(values);
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
      init: async () => {
        // TODO: Handle failures
        await controller.getLookups();
      },
    }),
    [
      dispatch,
      store,
      cache,
      lookups,
      actions,
      topics,
      rules,
      products,
      sources,
      licenses,
      ingestTypes,
      roles,
      series,
      contributors,
      sourceActions,
      sourceMetrics,
      tags,
      tonePools,
      users,
      dataLocations,
      settings,
      ministers,
    ],
  );

  return [state, controller];
};
