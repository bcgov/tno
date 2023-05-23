import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';
import { IMinisterModel } from 'store/hooks/subscriber/interfaces/IMinisterModel';
import {
  IActionModel,
  ICacheModel,
  IContributorModel,
  IHolidayModel,
  ILicenseModel,
  IProductModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceModel,
  ITagModel,
  ITonePoolModel,
  ITopicModel,
} from 'tno-core';

import {
  storeActions,
  storeCache,
  storeContributors,
  storeHolidays,
  storeLicenses,
  storeMinisters,
  storeProducts,
  storeSeries,
  storeSourceActions,
  storeSources,
  storeTags,
  storeTonePools,
  storeTopics,
  updateCache,
} from '.';
import { ILookupState } from './interfaces';

export interface ILookupStore {
  storeCache: (cache: ICacheModel[]) => void;
  updateCache: (cache: ICacheModel) => void;
  storeActions: (actions: IActionModel[]) => void;
  storeTopics: (topics: ITopicModel[]) => void;
  storeProducts: (contentTypes: IProductModel[]) => void;
  storeSources: (sources: ISourceModel[]) => void;
  storeLicenses: (licenses: ILicenseModel[]) => void;
  storeSeries: (series: ISeriesModel[]) => void;
  storeContributors: (contributors: IContributorModel[]) => void;
  storeSourceActions: (actions: ISourceActionModel[]) => void;
  storeTags: (tags: ITagModel[]) => void;
  storeTonePools: (contentTypes: ITonePoolModel[]) => void;
  storeHolidays: (users: IHolidayModel[]) => void;
  storeMinisters: (ministers: IMinisterModel[]) => void;
}

export const useLookupStore = (): [ILookupState, ILookupStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.lookup);

  const controller = React.useMemo(
    () => ({
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
      storeMinisters: (ministers: IMinisterModel[]) => {
        dispatch(storeMinisters(ministers));
      },
      storeProducts: (contentTypes: IProductModel[]) => {
        dispatch(storeProducts(contentTypes));
      },
      storeSources: (sources: ISourceModel[]) => {
        dispatch(storeSources(sources));
      },
      storeLicenses: (licenses: ILicenseModel[]) => {
        dispatch(storeLicenses(licenses));
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
      storeTags: (tags: ITagModel[]) => {
        dispatch(storeTags(tags));
      },
      storeTonePools: (tonePools: ITonePoolModel[]) => {
        dispatch(storeTonePools(tonePools));
      },
      storeHolidays: (holidays: IHolidayModel[]) => {
        dispatch(storeHolidays(holidays));
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
