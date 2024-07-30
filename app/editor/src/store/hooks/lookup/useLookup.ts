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
  IMediaTypeModel,
  IMetricModel,
  IMinisterModel,
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
  StorageKeys,
  useApiEditorActions,
  useApiEditorCache,
  useApiEditorContributors,
  useApiEditorDataLocations,
  useApiEditorIngestTypes,
  useApiEditorLicenses,
  useApiEditorLookups,
  useApiEditorMediaTypes,
  useApiEditorMetrics,
  useApiEditorMinisters,
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
  getMediaTypes: (refresh?: boolean) => Promise<IMediaTypeModel[]>;
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
  const mediaTypes = useApiEditorMediaTypes();
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
              saveToLocalStorage(StorageKeys.Actions, results.actions, store.storeActions);
              saveToLocalStorage(StorageKeys.Topics, results.topics, store.storeTopics);
              saveToLocalStorage(StorageKeys.Rules, results.rules, store.storeTopicScoreRules);
              saveToLocalStorage(StorageKeys.MediaTypes, results.mediaTypes, store.storeMediaTypes);
              saveToLocalStorage(StorageKeys.Sources, results.sources, store.storeSources);
              saveToLocalStorage(
                StorageKeys.IngestTypes,
                results.ingestTypes,
                store.storeIngestTypes,
              );
              saveToLocalStorage(StorageKeys.Licenses, results.licenses, store.storeLicenses);
              saveToLocalStorage(StorageKeys.Roles, results.roles, store.storeRoles);
              saveToLocalStorage(StorageKeys.Series, results.series, store.storeSeries);
              saveToLocalStorage(
                StorageKeys.Contributors,
                results.contributors,
                store.storeContributors,
              );
              saveToLocalStorage(
                StorageKeys.SourceActions,
                results.sourceActions,
                store.storeSourceActions,
              );
              saveToLocalStorage(StorageKeys.Metrics, results.metrics, store.storeMetrics);
              saveToLocalStorage(StorageKeys.Tags, results.tags, store.storeTags);
              saveToLocalStorage(StorageKeys.TonePools, results.tonePools, store.storeTonePools);
              saveToLocalStorage(StorageKeys.Users, results.users, store.storeUsers);
              saveToLocalStorage(
                StorageKeys.DataLocations,
                results.dataLocations,
                store.storeDataLocations,
              );
              saveToLocalStorage(StorageKeys.Settings, results.settings, store.storeSettings);
              saveToLocalStorage(StorageKeys.Holidays, results.holidays, store.storeHolidays);
              saveToLocalStorage(StorageKeys.Ministers, results.ministers, store.storeMinisters);
              return results;
            } else {
              const lookups = {
                actions: getFromLocalStorage<IActionModel[]>(StorageKeys.Actions, []),
                topics: getFromLocalStorage<ITopicModel[]>(StorageKeys.Topics, []),
                rules: getFromLocalStorage<ITopicScoreRuleModel[]>(StorageKeys.Rules, []),
                mediaTypes: getFromLocalStorage<IMediaTypeModel[]>(StorageKeys.MediaTypes, []),
                sources: getFromLocalStorage<ISourceModel[]>(StorageKeys.Sources, []),
                ingestTypes: getFromLocalStorage<IIngestTypeModel[]>(StorageKeys.IngestTypes, []),
                licenses: getFromLocalStorage<ILicenseModel[]>(StorageKeys.Licenses, []),
                roles: getFromLocalStorage<IRoleModel[]>(StorageKeys.Roles, []),
                series: getFromLocalStorage<ISeriesModel[]>(StorageKeys.Series, []),
                contributors: getFromLocalStorage<IContributorModel[]>(
                  StorageKeys.Contributors,
                  [],
                ),
                sourceActions: getFromLocalStorage<ISourceActionModel[]>(
                  StorageKeys.SourceActions,
                  [],
                ),
                metrics: getFromLocalStorage<IMetricModel[]>(StorageKeys.Metrics, []),
                tags: getFromLocalStorage<ITagModel[]>(StorageKeys.Tags, []),
                tonePools: getFromLocalStorage<ITonePoolModel[]>(StorageKeys.TonePools, []),
                users: getFromLocalStorage<IUserModel[]>(StorageKeys.Users, []),
                dataLocations: getFromLocalStorage<IDataLocationModel[]>(
                  StorageKeys.DataLocations,
                  [],
                ),
                settings: getFromLocalStorage<ISettingModel[]>(StorageKeys.Settings, []),
                holidays: getFromLocalStorage<IHolidayModel[]>(StorageKeys.Holidays, []),
                ministers: getFromLocalStorage<IMinisterModel[]>(StorageKeys.Ministers, []),
                systemMessages: [],
              };
              store.storeActions(lookups.actions);
              store.storeTopics(lookups.topics);
              store.storeTopicScoreRules(lookups.rules);
              store.storeMediaTypes(lookups.mediaTypes);
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
          StorageKeys.Actions,
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
      getTopics: async (refresh?: boolean) => {
        return await fetchIfNoneMatch<ITopicModel[]>(
          StorageKeys.Topics,
          dispatch,
          (etag) => topics.getTopics(etag),
          (results) => {
            const values = results ?? [];
            store.storeTopics(values);
            if (!!refresh) saveToLocalStorage(StorageKeys.Topics, values, store.storeTopics);
            return values;
          },
          true,
          'lookup',
        );
      },
      getTopicScoreRules: async () => {
        return await fetchIfNoneMatch<ITopicScoreRuleModel[]>(
          StorageKeys.Rules,
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
      getMediaTypes: async () => {
        return await fetchIfNoneMatch<IMediaTypeModel[]>(
          StorageKeys.MediaTypes,
          dispatch,
          (etag) => mediaTypes.getMediaTypes(etag),
          (results) => {
            const values = results ?? [];
            store.storeMediaTypes(values);
            return values;
          },
          true,
          'lookup',
        );
      },
      getSources: async () => {
        return await fetchIfNoneMatch<ISourceModel[]>(
          StorageKeys.Sources,
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
          StorageKeys.Licenses,
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
          StorageKeys.IngestTypes,
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
          StorageKeys.Roles,
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
          StorageKeys.Series,
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
          StorageKeys.Contributors,
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
          StorageKeys.SourceActions,
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
          StorageKeys.Metrics,
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
          StorageKeys.Tags,
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
          StorageKeys.TonePools,
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
          StorageKeys.Users,
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
          StorageKeys.DataLocations,
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
          StorageKeys.Settings,
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
          StorageKeys.Ministers,
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
        store.storeIsReady(true);
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
      mediaTypes,
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
