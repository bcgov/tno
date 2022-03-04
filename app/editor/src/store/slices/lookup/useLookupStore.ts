import {
  IActionModel,
  ICategoryModel,
  IContentTypeModel,
  IMediaTypeModel,
  ISeriesModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
} from 'hooks';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';
import { useDeepCompareMemo } from 'tno-core';

import {
  storeActions,
  storeCategories,
  storeContentTypes,
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
  storeMediaTypes: (mediaTypes: IMediaTypeModel[]) => void;
  storeSeries: (series: ISeriesModel[]) => void;
  storeTags: (tags: ITagModel[]) => void;
  storeTonePools: (contentTypes: ITonePoolModel[]) => void;
  storeUsers: (users: IUserModel[]) => void;
}

export const useLookupStore = (): [ILookupState, ILookupStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.lookup);

  const _storeActions = React.useCallback(
    (actions: IActionModel[]) => {
      dispatch(storeActions(actions));
    },
    [dispatch],
  );

  const _storeCategories = React.useCallback(
    (categories: ICategoryModel[]) => {
      dispatch(storeCategories(categories));
    },
    [dispatch],
  );

  const _storeContentTypes = React.useCallback(
    (contentTypes: IContentTypeModel[]) => {
      dispatch(storeContentTypes(contentTypes));
    },
    [dispatch],
  );

  const _storeMediaTypes = React.useCallback(
    (mediaTypes: IMediaTypeModel[]) => {
      dispatch(storeMediaTypes(mediaTypes));
    },
    [dispatch],
  );

  const _storeSeries = React.useCallback(
    (series: ISeriesModel[]) => {
      dispatch(storeSeries(series));
    },
    [dispatch],
  );

  const _storeTags = React.useCallback(
    (tags: ITagModel[]) => {
      dispatch(storeTags(tags));
    },
    [dispatch],
  );

  const _storeTonePools = React.useCallback(
    (tonePools: ITonePoolModel[]) => {
      dispatch(storeTonePools(tonePools));
    },
    [dispatch],
  );

  const _storeUsers = React.useCallback(
    (users: IUserModel[]) => {
      dispatch(storeUsers(users));
    },
    [dispatch],
  );

  return [
    state,
    useDeepCompareMemo(
      () => ({
        storeActions: _storeActions,
        storeCategories: _storeCategories,
        storeContentTypes: _storeContentTypes,
        storeMediaTypes: _storeMediaTypes,
        storeSeries: _storeSeries,
        storeTags: _storeTags,
        storeTonePools: _storeTonePools,
        storeUsers: _storeUsers,
      }),
      [
        _storeActions,
        _storeCategories,
        _storeContentTypes,
        _storeMediaTypes,
        _storeSeries,
        _storeTags,
        _storeTonePools,
        _storeUsers,
      ],
    ),
  ];
};
