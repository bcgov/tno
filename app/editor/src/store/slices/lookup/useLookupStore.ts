import {
  IActionModel,
  ICategoryModel,
  IContentTypeModel,
  ILicenseModel,
  IMediaTypeModel,
  ISeriesModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
} from 'hooks/api-editor';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import {
  storeActions,
  storeCategories,
  storeContentTypes,
  storeLicenses,
  storeMediaTypes,
  storeSeries,
  storeTags,
  storeTonePools,
  storeUsers,
} from '.';
import { ILookupState } from './interfaces';

export interface ILookupStore {
  storeActions: (actions: IActionModel[]) => void;
  storeCategories: (categories: ICategoryModel[]) => void;
  storeContentTypes: (contentTypes: IContentTypeModel[]) => void;
  storeLicenses: (licenses: ILicenseModel[]) => void;
  storeMediaTypes: (mediaTypes: IMediaTypeModel[]) => void;
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
      storeActions: (actions: IActionModel[]) => {
        dispatch(storeActions(actions));
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
