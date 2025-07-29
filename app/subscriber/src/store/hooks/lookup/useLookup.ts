import React from 'react';
import { ILookupState, useLookupStore } from 'store/slices';
import {
  getFromLocalStorage,
  IActionModel,
  IContributorModel,
  IDataLocationModel,
  IHolidayModel,
  IIngestTypeModel,
  ILicenseModel,
  ILookupModel,
  IMediaTypeModel,
  IMetricModel,
  IOrganizationModel,
  IRoleModel,
  ISeriesModel,
  ISettingModel,
  ISourceActionModel,
  ISourceModel,
  ISystemMessageModel,
  ITagModel,
  ITonePoolModel,
  ITopicModel,
  ITopicScoreRuleModel,
  IUserModel,
  StorageKeys,
  useApiSubscriberCache,
  useApiSubscriberMinisters,
} from 'tno-core';

import { useAjaxWrapper } from '..';
import { IMinisterModel } from '../subscriber/interfaces/IMinisterModel';
import { useApiLookups } from './useApiLookups';
import { fetchIfNoneMatch, saveToLocalStorage } from './utils';

export interface ILookupController {
  getLookups: () => Promise<ILookupModel>;
  getMinisters: (refresh?: boolean) => Promise<IMinisterModel[]>;
  init: (refresh?: boolean) => Promise<void>;
}

export const useLookup = (): [ILookupState, ILookupController] => {
  const [state, store] = useLookupStore();
  const dispatch = useAjaxWrapper();
  const cache = useApiSubscriberCache();
  const lookups = useApiLookups();
  const ministers = useApiSubscriberMinisters();

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
              saveToLocalStorage(
                StorageKeys.Contributors,
                results.contributors,
                store.storeContributors,
              );
              saveToLocalStorage(StorageKeys.Ministers, results.ministers, store.storeMinisters);
              saveToLocalStorage(StorageKeys.Topics, results.topics, store.storeTopics);
              saveToLocalStorage(StorageKeys.MediaTypes, results.mediaTypes, store.storeMediaTypes);
              saveToLocalStorage(StorageKeys.Sources, results.sources, store.storeSources);
              saveToLocalStorage(StorageKeys.Licenses, results.licenses, store.storeLicenses);
              saveToLocalStorage(StorageKeys.Series, results.series, store.storeSeries);
              saveToLocalStorage(StorageKeys.Tags, results.tags, store.storeTags);
              saveToLocalStorage(StorageKeys.Settings, results.settings, store.storeSettings);
              saveToLocalStorage(StorageKeys.TonePools, results.tonePools, store.storeTonePools);
              saveToLocalStorage(
                StorageKeys.SystemMessages,
                results.systemMessages,
                store.storeSystemMessages,
              );
              return results;
            } else {
              const lookups: ILookupModel = {
                actions: getFromLocalStorage<IActionModel[]>(StorageKeys.Actions, []),
                topics: getFromLocalStorage<ITopicModel[]>(StorageKeys.Topics, []),
                mediaTypes: getFromLocalStorage<IMediaTypeModel[]>(StorageKeys.MediaTypes, []),
                sources: getFromLocalStorage<ISourceModel[]>(StorageKeys.Sources, []),
                licenses: getFromLocalStorage<ILicenseModel[]>(StorageKeys.Licenses, []),
                series: getFromLocalStorage<ISeriesModel[]>(StorageKeys.Series, []),
                ministers: getFromLocalStorage<IMinisterModel[]>(StorageKeys.Ministers, []),
                tags: getFromLocalStorage<ITagModel[]>(StorageKeys.Tags, []),
                tonePools: getFromLocalStorage<ITonePoolModel[]>(StorageKeys.TonePools, []),
                rules: getFromLocalStorage<ITopicScoreRuleModel[]>(StorageKeys.Rules, []),
                ingestTypes: getFromLocalStorage<IIngestTypeModel[]>(StorageKeys.IngestTypes, []),
                roles: getFromLocalStorage<IRoleModel[]>(StorageKeys.Roles, []),
                organizations: getFromLocalStorage<IOrganizationModel[]>(StorageKeys.Roles, []),
                contributors: getFromLocalStorage<IContributorModel[]>(
                  StorageKeys.Contributors,
                  [],
                ),
                sourceActions: getFromLocalStorage<ISourceActionModel[]>(
                  StorageKeys.SourceActions,
                  [],
                ),
                metrics: getFromLocalStorage<IMetricModel[]>(StorageKeys.Metrics, []),
                users: getFromLocalStorage<IUserModel[]>(StorageKeys.Users, []),
                dataLocations: getFromLocalStorage<IDataLocationModel[]>(
                  StorageKeys.DataLocations,
                  [],
                ),
                settings: getFromLocalStorage<ISettingModel[]>(StorageKeys.Settings, []),
                holidays: getFromLocalStorage<IHolidayModel[]>(StorageKeys.Holidays, []),
                systemMessages: getFromLocalStorage<ISystemMessageModel[]>(
                  StorageKeys.SystemMessages,
                  [],
                ),
              };
              store.storeActions(lookups.actions);
              store.storeTopics(lookups.topics);
              store.storeTopicScoreRules(lookups.rules);
              store.storeMediaTypes(lookups.mediaTypes);
              store.storeSources(lookups.sources);
              store.storeLicenses(lookups.licenses);
              store.storeSeries(lookups.series);
              store.storeMinisters(lookups.ministers);
              store.storeTags(lookups.tags);
              store.storeTonePools(lookups.tonePools);
              store.storeIngestTypes(lookups.ingestTypes);
              store.storeRoles(lookups.roles);
              store.storeContributors(lookups.contributors);
              store.storeSourceActions(lookups.sourceActions);
              store.storeMetrics(lookups.metrics);
              store.storeUsers(lookups.users);
              store.storeDataLocations(lookups.dataLocations);
              store.storeSettings(lookups.settings);
              store.storeHolidays(lookups.holidays);
              store.storeSystemMessages(lookups.systemMessages);
              return lookups;
            }
          },
          false,
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
        store.storeIsReady(true);
      },
    }),
    [cache, dispatch, lookups, ministers, store],
  );

  return [state, controller];
};
