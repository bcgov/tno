import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import { storeSettingsLoading, storeSettingsValues } from '.';
import { ISettingsState } from './interfaces';

export interface ISettingsStore {
  storeLoading: (value: number) => void;
  storeValues: (cache: ISettingsState) => void;
}

export const useSettingsStore = (): [ISettingsState, ISettingsStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.settings);

  const controller = React.useMemo(
    () => ({
      storeLoading: (value: number) => {
        dispatch(storeSettingsLoading(value));
      },
      storeValues: (values: ISettingsState) => {
        dispatch(storeSettingsValues(values));
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
