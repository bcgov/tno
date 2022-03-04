import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';
import { useDeepCompareMemo } from 'tno-core';

import { addRequest, clearRequests, removeRequest } from '.';
import { IAppState } from './interfaces';

export interface IAppStore {
  addRequest: (url: string) => void;
  removeRequest: (url: string) => void;
  clearRequests: () => void;
  isLoading: () => boolean;
}

export const useAppStore = (): [IAppState, IAppStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.app);

  const _addRequest = React.useCallback(
    (url: string) => {
      dispatch(addRequest(url));
    },
    [dispatch],
  );

  const _removeRequest = React.useCallback(
    (url: string) => {
      dispatch(removeRequest(url));
    },
    [dispatch],
  );

  const _clearRequests = React.useCallback(() => {
    dispatch(clearRequests());
  }, [dispatch]);

  const _isLoading = React.useCallback(() => {
    return !!state.requests.length;
  }, [state.requests.length]);

  const controller = useDeepCompareMemo(
    () => ({
      addRequest: _addRequest,
      removeRequest: _removeRequest,
      clearRequests: _clearRequests,
      isLoading: _isLoading,
    }),
    [_addRequest, _removeRequest, _clearRequests, _isLoading],
  );

  return [state, controller];
};
