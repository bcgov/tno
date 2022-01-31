import { IContentTypeModel, IMediaTypeModel, IUserModel, useDeepCompareMemo } from 'hooks';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import { storeContentTypes, storeMediaTypes, storeUsers } from '.';
import { ILookupState } from './interfaces';

export interface ILookupStore {
  storeContentTypes: (contentTypes: IContentTypeModel[]) => void;
  storeMediaTypes: (mediaTypes: IMediaTypeModel[]) => void;
  storeUsers: (users: IUserModel[]) => void;
  state: ILookupState;
}

export const useLookup = (): ILookupStore => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.lookup);

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

  const _storeUsers = React.useCallback(
    (users: IUserModel[]) => {
      dispatch(storeUsers(users));
    },
    [dispatch],
  );

  return useDeepCompareMemo(
    () => ({
      storeContentTypes: _storeContentTypes,
      storeMediaTypes: _storeMediaTypes,
      storeUsers: _storeUsers,
      state: state,
    }),
    [_storeMediaTypes, _storeUsers, state],
  );
};
