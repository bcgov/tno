import React from 'react';
import { useStore } from 'react-redux';

import {
  IContributorModel,
  IFilterModel,
  IFolderModel,
  IReportModel,
  ISubscriberUserModel,
  ISystemMessageModel,
  ITonePoolModel,
  IUserColleagueModel,
} from '@/shared';
import { IReportResultForm } from '@/subscriber/features/my-reports/interfaces';
import { ActionDelegate, RootState, useAppDispatch, useAppSelector } from '@/subscriber/store';

import {
  storeFilter,
  storeFrom,
  storeImpersonate,
  storeMyColleagues,
  storeMyFilters,
  storeMyFolders,
  storeMyMessages,
  storeMyProfile,
  storeMyReports,
  storeMyTonePool,
  storeReportContent,
  storeReportOutput,
  storeReportsFilter,
} from '.';
import { storeContributors } from '../lookup';
import { IProfileState } from './interfaces';

export interface IProfileStore {
  storeMyProfile: (
    user: ISubscriberUserModel | ActionDelegate<ISubscriberUserModel | undefined> | undefined,
  ) => void;
  storeImpersonate: (
    user: ISubscriberUserModel | ActionDelegate<ISubscriberUserModel | undefined> | undefined,
  ) => void;
  storeContributors: (
    contributors: IContributorModel[] | ActionDelegate<IContributorModel[]>,
  ) => void;
  storeFrom: (from: number | ActionDelegate<number>) => void;
  storeFilter: (filter: IFilterModel | ActionDelegate<IFilterModel | undefined>) => void;
  storeMyFilters: (filters: IFilterModel[] | ActionDelegate<IFilterModel[]>) => void;
  storeMyFolders: (folders: IFolderModel[] | ActionDelegate<IFolderModel[]>) => void;
  storeMyReports: (reports: IReportModel[] | ActionDelegate<IReportModel[]>) => void;
  storeMyColleagues: (
    reports: IUserColleagueModel[] | ActionDelegate<IUserColleagueModel[]>,
  ) => void;
  storeReportsFilter: (filter: string | ActionDelegate<string>) => void;
  storeReportOutput: (
    output: IReportResultForm | undefined | ActionDelegate<IReportResultForm | undefined>,
  ) => void;
  storeReportContent: (
    output: { [reportId: number]: number[] } | ActionDelegate<{ [reportId: number]: number[] }>,
  ) => void;
  storeMyMessages: (
    messages: ISystemMessageModel[] | ActionDelegate<ISystemMessageModel[]>,
  ) => void;
  storeMyTonePool: (tonePool: ITonePoolModel | ActionDelegate<ITonePoolModel | undefined>) => void;
}

export const useProfileStore = (): [IProfileState, IProfileStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.profile);
  // Updates given as a function apply to the profile as it is when they are made. The 'state' this
  // render selected can be out of date by then: two requests started together would each apply their
  // update to the same old value, and the last to finish would discard the other's.
  const store = useStore<RootState>();

  const controller = React.useMemo(
    () => ({
      storeMyProfile: (
        user: ISubscriberUserModel | ActionDelegate<ISubscriberUserModel | undefined> | undefined,
      ) => {
        if (typeof user === 'function') {
          dispatch(storeMyProfile(user(store.getState().profile.profile)));
        } else dispatch(storeMyProfile(user));
      },
      storeImpersonate: (
        user: ISubscriberUserModel | ActionDelegate<ISubscriberUserModel | undefined> | undefined,
      ) => {
        if (typeof user === 'function') {
          dispatch(storeImpersonate(user(store.getState().profile.impersonate)));
        } else dispatch(storeImpersonate(user));
      },
      storeFrom: (from: number | ActionDelegate<number>) => {
        if (typeof from === 'function') {
          dispatch(storeFrom(from(store.getState().profile.from)));
        } else dispatch(storeFrom(from));
      },
      storeFilter: (filter: IFilterModel | ActionDelegate<IFilterModel | undefined>) => {
        if (typeof filter === 'function') {
          dispatch(storeFilter(filter(store.getState().profile.filter)));
        } else dispatch(storeFilter(filter));
      },
      storeMyFilters: (filters: IFilterModel[] | ActionDelegate<IFilterModel[]>) => {
        if (typeof filters === 'function') {
          dispatch(storeMyFilters(filters(store.getState().profile.myFilters)));
        } else dispatch(storeMyFilters(filters));
      },
      storeMyFolders: (folders: IFolderModel[] | ActionDelegate<IFolderModel[]>) => {
        if (typeof folders === 'function') {
          dispatch(storeMyFolders(folders(store.getState().profile.myFolders)));
        } else dispatch(storeMyFolders(folders));
      },
      storeMyReports: (reports: IReportModel[] | ActionDelegate<IReportModel[]>) => {
        if (typeof reports === 'function') {
          dispatch(storeMyReports(reports(store.getState().profile.myReports)));
        } else dispatch(storeMyReports(reports));
      },
      storeMyColleagues: (
        colleagues: IUserColleagueModel[] | ActionDelegate<IUserColleagueModel[]>,
      ) => {
        if (typeof colleagues === 'function') {
          dispatch(storeMyColleagues(colleagues(store.getState().profile.myColleagues)));
        } else dispatch(storeMyColleagues(colleagues));
      },
      storeReportsFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeReportsFilter(filter(store.getState().profile.reportsFilter)));
        } else dispatch(storeReportsFilter(filter));
      },
      storeReportOutput: (
        output: IReportResultForm | undefined | ActionDelegate<IReportResultForm | undefined>,
      ) => {
        if (typeof output === 'function') {
          dispatch(storeReportOutput(output(store.getState().profile.reportOutput)));
        } else dispatch(storeReportOutput(output));
      },
      storeReportContent: (
        reportContent:
          { [reportId: number]: number[] } | ActionDelegate<{ [reportId: number]: number[] }>,
      ) => {
        if (typeof reportContent === 'function') {
          dispatch(storeReportContent(reportContent(store.getState().profile.reportContent)));
        } else dispatch(storeReportContent(reportContent));
      },
      storeContributors: (
        contributors: IContributorModel[] | ActionDelegate<IContributorModel[]>,
      ) => {
        if (typeof contributors === 'function') {
          dispatch(storeContributors(contributors(store.getState().profile.contributors)));
        } else dispatch(storeContributors(contributors));
      },
      storeMyMessages: (
        messages: ISystemMessageModel[] | ActionDelegate<ISystemMessageModel[]>,
      ) => {
        if (typeof messages === 'function') {
          dispatch(storeMyMessages(messages(store.getState().profile.messages)));
        } else dispatch(storeMyMessages(messages));
      },
      storeMyTonePool: (tonePool: ITonePoolModel | ActionDelegate<ITonePoolModel | undefined>) => {
        if (typeof tonePool === 'function') {
          const updatedTonePool = tonePool(store.getState().profile.myTonePool);
          if (updatedTonePool !== undefined) {
            dispatch(storeMyTonePool(updatedTonePool));
          }
        } else if (tonePool !== undefined) {
          dispatch(storeMyTonePool(tonePool));
        }
      },
    }),
    [dispatch, store],
  );

  return [state, controller];
};
