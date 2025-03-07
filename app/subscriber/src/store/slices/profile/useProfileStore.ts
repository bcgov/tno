import { IReportResultForm } from 'features/my-reports/interfaces';
import React from 'react';
import { ActionDelegate, useAppDispatch, useAppSelector } from 'store';
import {
  IContributorModel,
  IFilterModel,
  IFolderModel,
  IReportModel,
  ISubscriberUserModel,
  ISystemMessageModel,
  ITonePoolModel,
  IUserColleagueModel,
} from 'tno-core';

import { storeContributors } from '../lookup';
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

  const controller = React.useMemo(
    () => ({
      storeMyProfile: (
        user: ISubscriberUserModel | ActionDelegate<ISubscriberUserModel | undefined> | undefined,
      ) => {
        if (typeof user === 'function') {
          dispatch(storeMyProfile(user(state.profile)));
        } else dispatch(storeMyProfile(user));
      },
      storeImpersonate: (
        user: ISubscriberUserModel | ActionDelegate<ISubscriberUserModel | undefined> | undefined,
      ) => {
        if (typeof user === 'function') {
          dispatch(storeImpersonate(user(state.impersonate)));
        } else dispatch(storeImpersonate(user));
      },
      storeFrom: (from: number | ActionDelegate<number>) => {
        if (typeof from === 'function') {
          dispatch(storeFrom(from(state.from)));
        } else dispatch(storeFrom(from));
      },
      storeFilter: (filter: IFilterModel | ActionDelegate<IFilterModel | undefined>) => {
        if (typeof filter === 'function') {
          dispatch(storeFilter(filter(state.filter)));
        } else dispatch(storeFilter(filter));
      },
      storeMyFilters: (filters: IFilterModel[] | ActionDelegate<IFilterModel[]>) => {
        if (typeof filters === 'function') {
          dispatch(storeMyFilters(filters(state.myFilters)));
        } else dispatch(storeMyFilters(filters));
      },
      storeMyFolders: (folders: IFolderModel[] | ActionDelegate<IFolderModel[]>) => {
        if (typeof folders === 'function') {
          dispatch(storeMyFolders(folders(state.myFolders)));
        } else dispatch(storeMyFolders(folders));
      },
      storeMyReports: (reports: IReportModel[] | ActionDelegate<IReportModel[]>) => {
        if (typeof reports === 'function') {
          dispatch(storeMyReports(reports(state.myReports)));
        } else dispatch(storeMyReports(reports));
      },
      storeMyColleagues: (
        colleagues: IUserColleagueModel[] | ActionDelegate<IUserColleagueModel[]>,
      ) => {
        if (typeof colleagues === 'function') {
          dispatch(storeMyColleagues(colleagues(state.myColleagues)));
        } else dispatch(storeMyColleagues(colleagues));
      },
      storeReportsFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeReportsFilter(filter(state.reportsFilter)));
        } else dispatch(storeReportsFilter(filter));
      },
      storeReportOutput: (
        output: IReportResultForm | undefined | ActionDelegate<IReportResultForm | undefined>,
      ) => {
        if (typeof output === 'function') {
          dispatch(storeReportOutput(output(state.reportOutput)));
        } else dispatch(storeReportOutput(output));
      },
      storeReportContent: (
        reportContent:
          | { [reportId: number]: number[] }
          | ActionDelegate<{ [reportId: number]: number[] }>,
      ) => {
        if (typeof reportContent === 'function') {
          dispatch(storeReportContent(reportContent(state.reportContent)));
        } else dispatch(storeReportContent(reportContent));
      },
      storeContributors: (
        contributors: IContributorModel[] | ActionDelegate<IContributorModel[]>,
      ) => {
        if (typeof contributors === 'function') {
          dispatch(storeContributors(contributors(state.contributors)));
        } else dispatch(storeContributors(contributors));
      },
      storeMyMessages: (
        messages: ISystemMessageModel[] | ActionDelegate<ISystemMessageModel[]>,
      ) => {
        if (typeof messages === 'function') {
          dispatch(storeMyMessages(messages(state.messages)));
        } else dispatch(storeMyMessages(messages));
      },
      storeMyTonePool: (tonePool: ITonePoolModel | ActionDelegate<ITonePoolModel | undefined>) => {
        if (typeof tonePool === 'function') {
          const updatedTonePool = tonePool(state.myTonePool);
          if (updatedTonePool !== undefined) {
            dispatch(storeMyTonePool(updatedTonePool));
          }
        } else if (tonePool !== undefined) {
          dispatch(storeMyTonePool(tonePool));
        }
      },
    }),
    [
      dispatch,
      state.profile,
      state.impersonate,
      state.from,
      state.filter,
      state.myFilters,
      state.myFolders,
      state.myReports,
      state.myColleagues,
      state.reportsFilter,
      state.reportOutput,
      state.reportContent,
      state.contributors,
      state.messages,
      state.myTonePool,
    ],
  );

  return [state, controller];
};
