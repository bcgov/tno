import {
  IActionModel,
  ICacheModel,
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
} from 'hooks/api-editor';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import {
  storeActions,
  storeCache,
  storeCategories,
  storeContentTypes,
  storeDataSources,
  storeLicenses,
  storeMediaTypes,
  storeSeries,
  storeSourceActions,
  storeSourceMetrics,
  storeTags,
  storeTonePools,
  storeUsers,
  updateCache,
} from '.';
import { ILookupState } from './interfaces';

export interface ILookupStore {
  storeCache: (cache: ICacheModel[]) => void;
  updateCache: (cache: ICacheModel) => void;
  storeActions: (actions: IActionModel[]) => void;
  storeSourceActions: (actions: ISourceActionModel[]) => void;
  storeSourceMetrics: (metrics: ISourceMetricModel[]) => void;
  storeCategories: (categories: ICategoryModel[]) => void;
  storeContentTypes: (contentTypes: IContentTypeModel[]) => void;
  storeLicenses: (licenses: ILicenseModel[]) => void;
  storeMediaTypes: (mediaTypes: IMediaTypeModel[]) => void;
  storeDataSources: (dataSources: IDataSourceModel[]) => void;
  storeSeries: (series: ISeriesModel[]) => void;
  storeTags: (tags: ITagModel[]) => void;
  storeTonePools: (contentTypes: ITonePoolModel[]) => void;
  storeUsers: (users: IUserModel[]) => void;
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
      storeSourceActions: (actions: ISourceActionModel[]) => {
        dispatch(storeSourceActions(actions));
      },
      storeSourceMetrics: (metrics: ISourceMetricModel[]) => {
        dispatch(storeSourceMetrics(metrics));
      },
      storeCategories: (categories: ICategoryModel[]) => {
        dispatch(storeCategories(categories));
      },
      storeContentTypes: (contentTypes: IContentTypeModel[]) => {
        dispatch(storeContentTypes(contentTypes));
      },
      storeLicenses: (licenses: ILicenseModel[]) => {
        dispatch(storeLicenses(licenses));
      },
      storeMediaTypes: (mediaTypes: IMediaTypeModel[]) => {
        dispatch(storeMediaTypes(mediaTypes));
      },
      storeDataSources: (dataSources: IDataSourceModel[]) => {
        dispatch(storeDataSources(dataSources));
      },
      storeSeries: (series: ISeriesModel[]) => {
        dispatch(storeSeries(series));
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
    }),
    [dispatch],
  );

  return [state, controller];
};
