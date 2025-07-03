import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';
import {
  IActionModel,
  ICacheModel,
  IContributorModel,
  IDataLocationModel,
  IHolidayModel,
  IIngestTypeModel,
  ILicenseModel,
  IMediaTypeModel,
  IMetricModel,
  IMinisterModel,
  IOrganizationModel,
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
} from 'tno-core';

import {
  storeActions,
  storeCache,
  storeContributors,
  storeDataLocations,
  storeHolidays,
  storeIngestTypes,
  storeIsReady,
  storeLicenses,
  storeMediaTypes,
  storeMetrics,
  storeMinisters,
  storeOrganizations,
  storeRoles,
  storeSeries,
  storeSettings,
  storeSourceActions,
  storeSources,
  storeTags,
  storeTonePools,
  storeTopics,
  storeTopicScoreRules,
  storeUsers,
  updateCache,
} from '.';
import { ILookupState } from './interfaces';

export interface ILookupStore {
  storeIsReady: (isReady: boolean) => void;
  storeCache: (cache: ICacheModel[]) => void;
  updateCache: (cache: ICacheModel) => void;
  storeActions: (actions: IActionModel[]) => void;
  storeTopics: (topics: ITopicModel[]) => void;
  storeTopicScoreRules: (rules: ITopicScoreRuleModel[]) => void;
  storeMediaTypes: (contentTypes: IMediaTypeModel[]) => void;
  storeSources: (sources: ISourceModel[]) => void;
  storeLicenses: (licenses: ILicenseModel[]) => void;
  storeIngestTypes: (ingestTypes: IIngestTypeModel[]) => void;
  storeRoles: (roles: IRoleModel[]) => void;
  storeSeries: (series: ISeriesModel[]) => void;
  storeContributors: (contributors: IContributorModel[]) => void;
  storeSourceActions: (actions: ISourceActionModel[]) => void;
  storeMetrics: (metrics: IMetricModel[]) => void;
  storeTags: (tags: ITagModel[]) => void;
  storeTonePools: (contentTypes: ITonePoolModel[]) => void;
  storeUsers: (users: IUserModel[]) => void;
  storeDataLocations: (dataLocations: IDataLocationModel[]) => void;
  storeSettings: (settings: ISettingModel[]) => void;
  storeHolidays: (users: IHolidayModel[]) => void;
  storeMinisters: (ministers: IMinisterModel[]) => void;
  storeOrganizations: (organizations: IOrganizationModel[]) => void;
}

export const useLookupStore = (): [ILookupState, ILookupStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.lookup);

  const controller = React.useMemo(
    () => ({
      storeIsReady: (isReady: boolean) => {
        dispatch(storeIsReady(isReady));
      },
      storeCache: (cache: ICacheModel[]) => {
        dispatch(storeCache(cache));
      },
      updateCache: (cache: ICacheModel) => {
        dispatch(updateCache(cache));
      },
      storeActions: (actions: IActionModel[]) => {
        dispatch(storeActions(actions));
      },
      storeTopics: (topics: ITopicModel[]) => {
        dispatch(storeTopics(topics));
      },
      storeTopicScoreRules: (rules: ITopicScoreRuleModel[]) => {
        dispatch(storeTopicScoreRules(rules));
      },
      storeMediaTypes: (contentTypes: IMediaTypeModel[]) => {
        dispatch(storeMediaTypes(contentTypes));
      },
      storeSources: (sources: ISourceModel[]) => {
        dispatch(storeSources(sources));
      },
      storeLicenses: (licenses: ILicenseModel[]) => {
        dispatch(storeLicenses(licenses));
      },
      storeIngestTypes: (ingestTypes: IIngestTypeModel[]) => {
        dispatch(storeIngestTypes(ingestTypes));
      },
      storeRoles: (roles: IRoleModel[]) => {
        dispatch(storeRoles(roles));
      },
      storeSeries: (series: ISeriesModel[]) => {
        dispatch(storeSeries(series));
      },
      storeContributors: (contributors: IContributorModel[]) => {
        dispatch(storeContributors(contributors));
      },
      storeSourceActions: (actions: ISourceActionModel[]) => {
        dispatch(storeSourceActions(actions));
      },
      storeMetrics: (metrics: IMetricModel[]) => {
        dispatch(storeMetrics(metrics));
      },
      storeTags: (tags: ITagModel[]) => {
        dispatch(storeTags(tags));
      },
      storeTonePools: (tonePools: ITonePoolModel[]) => {
        dispatch(storeTonePools(tonePools));
      },
      storeUsers: (users: IUserModel[]) => {
        dispatch(storeUsers(users));
      },
      storeDataLocations: (dataLocations: IDataLocationModel[]) => {
        dispatch(storeDataLocations(dataLocations));
      },
      storeSettings: (settings: ISettingModel[]) => {
        dispatch(storeSettings(settings));
      },
      storeHolidays: (holidays: IHolidayModel[]) => {
        dispatch(storeHolidays(holidays));
      },
      storeMinisters: (ministers: IMinisterModel[]) => {
        dispatch(storeMinisters(ministers));
      },
      storeOrganizations: (organizations: IOrganizationModel[]) => {
        dispatch(storeOrganizations(organizations));
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
