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
  IMetricModel,
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
              saveToLocalStorage('actions', results.actions, store.storeActions);
              saveToLocalStorage('ministers', results.ministers, store.storeMinisters);
              saveToLocalStorage('topics', results.topics, store.storeTopics);
              saveToLocalStorage('products', results.products, store.storeProducts);
              saveToLocalStorage('sources', results.sources, store.storeSources);
              saveToLocalStorage('licenses', results.licenses, store.storeLicenses);
              saveToLocalStorage('series', results.series, store.storeSeries);
              saveToLocalStorage('tags', results.tags, store.storeTags);
              saveToLocalStorage('tone_pools', results.tonePools, store.storeTonePools);
              return results;
            } else {
              const lookups: ILookupModel = {
                actions: getFromLocalStorage<IActionModel[]>('actions', []),
                topics: getFromLocalStorage<ITopicModel[]>('topics', []),
                products: getFromLocalStorage<IProductModel[]>('products', []),
                sources: getFromLocalStorage<ISourceModel[]>('sources', []),
                licenses: getFromLocalStorage<ILicenseModel[]>('licenses', []),
                series: getFromLocalStorage<ISeriesModel[]>('series', []),
                ministers: getFromLocalStorage<IMinisterModel[]>('ministers', []),
                tags: getFromLocalStorage<ITagModel[]>('tags', []),
                tonePools: getFromLocalStorage<ITonePoolModel[]>('tone_pools', []),
                rules: getFromLocalStorage<ITopicScoreRuleModel[]>('rules', []),
                ingestTypes: getFromLocalStorage<IIngestTypeModel[]>('ingest_types', []),
                roles: getFromLocalStorage<IRoleModel[]>('roles', []),
                contributors: getFromLocalStorage<IContributorModel[]>('contributors', []),
                sourceActions: getFromLocalStorage<ISourceActionModel[]>('source_actions', []),
                metrics: getFromLocalStorage<IMetricModel[]>('metrics', []),
                users: getFromLocalStorage<IUserModel[]>('users', []),
                dataLocations: getFromLocalStorage<IDataLocationModel[]>('data_locations', []),
                settings: getFromLocalStorage<ISettingModel[]>('settings', []),
                holidays: getFromLocalStorage<IHolidayModel[]>('holidays', []),
              };
              store.storeActions(lookups.actions);
              store.storeTopics(lookups.topics);
              store.storeTopicScoreRules(lookups.rules);
              store.storeProducts(lookups.products);
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
      },
    }),
    [cache, dispatch, lookups, ministers, store],
  );

  return [state, controller];
};
