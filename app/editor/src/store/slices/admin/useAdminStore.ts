import {
  IEveningOverviewItem,
  IEveningOverviewSection,
} from 'features/admin/evening-overview/interfaces';
import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import { IWorkOrderListFilter } from 'features/admin/work-orders/interfaces/IWorkOrderListFilter';
import React from 'react';
import { ActionDelegate, useAppDispatch, useAppSelector } from 'store';
import {
  IActionModel,
  IChartTemplateModel,
  IConnectionModel,
  IContributorModel,
  IDataLocationModel,
  IFilterModel,
  IFolderModel,
  IIngestModel,
  IIngestTypeModel,
  ILicenseModel,
  IMinisterModel,
  INotificationModel,
  IOrganizationModel,
  IPaged,
  IProductModel,
  IReportModel,
  IReportTemplateModel,
  ISeriesModel,
  ISettingModel,
  ISourceModel,
  ISystemMessageModel,
  ITagModel,
  ITopicModel,
  ITopicScoreRuleModel,
  IUserModel,
  IWorkOrderModel,
} from 'tno-core';

import {
  storeAdminActions,
  storeAdminChartTemplates,
  storeAdminConnections,
  storeAdminContributors,
  storeAdminDataLocations,
  storeAdminEveningOverviewItems,
  storeAdminEveningOverviewSections,
  storeAdminFilters,
  storeAdminFolders,
  storeAdminIngests,
  storeAdminIngestTypes,
  storeAdminLicenses,
  storeAdminMinisters,
  storeAdminNotifications,
  storeAdminOrganizations,
  storeAdminProducts,
  storeAdminReports,
  storeAdminReportTemplates,
  storeAdminSeries,
  storeAdminSettings,
  storeAdminSources,
  storeAdminSystemMessages,
  storeAdminTags,
  storeAdminTopics,
  storeAdminTopicScoreRules,
  storeAdminUserFilter,
  storeAdminUsers,
  storeAdminWorkOrderFilter,
  storeAdminWorkOrders,
} from '.';
import { IAdminState } from './interfaces';

export interface IAdminProps {}

export interface IAdminStore {
  storeActions: (actions: IActionModel[] | ActionDelegate<IActionModel[]>) => void;
  storeChartTemplates: (
    chartTemplates: IChartTemplateModel[] | ActionDelegate<IChartTemplateModel[]>,
  ) => void;
  storeConnections: (connections: IConnectionModel[] | ActionDelegate<IConnectionModel[]>) => void;
  storeContributors: (series: IContributorModel[] | ActionDelegate<IContributorModel[]>) => void;
  storeDataLocations: (
    dataLocations: IDataLocationModel[] | ActionDelegate<IDataLocationModel[]>,
  ) => void;
  storeEveningOverviewItems: (
    items: IEveningOverviewItem[] | ActionDelegate<IEveningOverviewItem[]>,
  ) => void;
  storeEveningOverviewSections: (
    sections: IEveningOverviewSection[] | ActionDelegate<IEveningOverviewSection[]>,
  ) => void;
  storeFolders: (folders: IFolderModel[] | ActionDelegate<IFolderModel[]>) => void;
  storeFilters: (filters: IFilterModel[] | ActionDelegate<IFilterModel[]>) => void;
  storeIngests: (ingests: IIngestModel[] | ActionDelegate<IIngestModel[]>) => void;
  storeIngestTypes: (ingestTypes: IIngestTypeModel[] | ActionDelegate<IIngestTypeModel[]>) => void;
  storeLicenses: (licenses: ILicenseModel[] | ActionDelegate<ILicenseModel[]>) => void;
  storeMinisters: (ministers: IMinisterModel[] | ActionDelegate<IMinisterModel[]>) => void;
  storeNotifications: (
    notifications: INotificationModel[] | ActionDelegate<INotificationModel[]>,
  ) => void;
  storeOrganizations: (
    organizations: IOrganizationModel[] | ActionDelegate<IOrganizationModel[]>,
  ) => void;
  storeProducts: (products: IProductModel[] | ActionDelegate<IProductModel[]>) => void;
  storeReports: (reports: IReportModel[] | ActionDelegate<IReportModel[]>) => void;
  storeReportTemplates: (
    reportTemplates: IReportTemplateModel[] | ActionDelegate<IReportTemplateModel[]>,
  ) => void;
  storeSeries: (series: ISeriesModel[] | ActionDelegate<ISeriesModel[]>) => void;
  storeSources: (sources: ISourceModel[] | ActionDelegate<ISourceModel[]>) => void;
  storeSystemMessages: (
    systemMessages: ISystemMessageModel[] | ActionDelegate<ISystemMessageModel[]>,
  ) => void;
  storeTags: (tags: ITagModel[] | ActionDelegate<ITagModel[]>) => void;
  storeTopics: (topics: ITopicModel[] | ActionDelegate<ITopicModel[]>) => void;
  storeTopicScoreRules: (
    rules: ITopicScoreRuleModel[] | ActionDelegate<ITopicScoreRuleModel[]>,
  ) => void;
  storeUserFilter: (filter: IUserListFilter | ActionDelegate<IUserListFilter>) => void;
  storeUsers: (users: IPaged<IUserModel> | ActionDelegate<IPaged<IUserModel>>) => void;
  storeWorkOrderFilter: (
    filter: IWorkOrderListFilter | ActionDelegate<IWorkOrderListFilter>,
  ) => void;
  storeWorkOrders: (
    workOrders: IPaged<IWorkOrderModel> | ActionDelegate<IPaged<IWorkOrderModel>>,
  ) => void;
  storeSettings: (settings: ISettingModel[] | ActionDelegate<ISettingModel[]>) => void;
}

export const useAdminStore = (props?: IAdminProps): [IAdminState, IAdminStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.admin);

  const controller = React.useMemo(
    () => ({
      storeActions: (actions: IActionModel[] | ActionDelegate<IActionModel[]>) => {
        if (typeof actions === 'function') {
          dispatch(storeAdminActions(actions(state.actions)));
        } else dispatch(storeAdminActions(actions));
      },
      storeChartTemplates: (
        chartTemplates: IChartTemplateModel[] | ActionDelegate<IChartTemplateModel[]>,
      ) => {
        if (typeof chartTemplates === 'function') {
          dispatch(storeAdminChartTemplates(chartTemplates(state.chartTemplates)));
        } else dispatch(storeAdminChartTemplates(chartTemplates));
      },
      storeConnections: (connections: IConnectionModel[] | ActionDelegate<IConnectionModel[]>) => {
        if (typeof connections === 'function') {
          dispatch(storeAdminConnections(connections(state.connections)));
        } else dispatch(storeAdminConnections(connections));
      },
      storeContributors: (
        contributors: IContributorModel[] | ActionDelegate<IContributorModel[]>,
      ) => {
        if (typeof contributors === 'function') {
          dispatch(storeAdminContributors(contributors(state.contributors)));
        } else dispatch(storeAdminContributors(contributors));
      },
      storeDataLocations: (
        dataLocations: IDataLocationModel[] | ActionDelegate<IDataLocationModel[]>,
      ) => {
        if (typeof dataLocations === 'function') {
          dispatch(storeAdminDataLocations(dataLocations(state.dataLocations)));
        } else dispatch(storeAdminDataLocations(dataLocations));
      },
      storeEveningOverviewSections: (
        eveningOverviewSections:
          | IEveningOverviewSection[]
          | ActionDelegate<IEveningOverviewSection[]>,
      ) => {
        if (typeof eveningOverviewSections === 'function') {
          dispatch(
            storeAdminEveningOverviewSections(
              eveningOverviewSections(eveningOverviewSections(state.eveningOverviewSections)),
            ),
          );
        } else dispatch(storeAdminEveningOverviewSections(eveningOverviewSections));
      },
      storeEveningOverviewItems: (
        eveningOverviewItems: IEveningOverviewItem[] | ActionDelegate<IEveningOverviewItem[]>,
      ) => {
        if (typeof eveningOverviewItems === 'function') {
          dispatch(
            storeAdminEveningOverviewItems(
              eveningOverviewItems(eveningOverviewItems(state.eveningOverviewItems)),
            ),
          );
        } else dispatch(storeAdminEveningOverviewItems(eveningOverviewItems));
      },
      storeFilters: (filters: IFilterModel[] | ActionDelegate<IFilterModel[]>) => {
        if (typeof filters === 'function') {
          dispatch(storeAdminFilters(filters(state.filters)));
        } else dispatch(storeAdminFilters(filters));
      },
      storeFolders: (folders: IFolderModel[] | ActionDelegate<IFolderModel[]>) => {
        if (typeof folders === 'function') {
          dispatch(storeAdminFolders(folders(state.folders)));
        } else dispatch(storeAdminFolders(folders));
      },
      storeIngests: (ingests: IIngestModel[] | ActionDelegate<IIngestModel[]>) => {
        if (typeof ingests === 'function') {
          dispatch(storeAdminIngests(ingests(state.ingests)));
        } else dispatch(storeAdminIngests(ingests));
      },
      storeIngestTypes: (ingestTypes: IIngestTypeModel[] | ActionDelegate<IIngestTypeModel[]>) => {
        if (typeof ingestTypes === 'function') {
          dispatch(storeAdminIngestTypes(ingestTypes(state.ingestTypes)));
        } else dispatch(storeAdminIngestTypes(ingestTypes));
      },
      storeLicenses: (licenses: ILicenseModel[] | ActionDelegate<ILicenseModel[]>) => {
        if (typeof licenses === 'function') {
          dispatch(storeAdminLicenses(licenses(state.licenses)));
        } else dispatch(storeAdminLicenses(licenses));
      },
      storeMinisters: (ministers: IMinisterModel[] | ActionDelegate<IMinisterModel[]>) => {
        if (typeof ministers === 'function') {
          dispatch(storeAdminMinisters(ministers(state.ministers)));
        } else dispatch(storeAdminMinisters(ministers));
      },
      storeNotifications: (
        notifications: INotificationModel[] | ActionDelegate<INotificationModel[]>,
      ) => {
        if (typeof notifications === 'function') {
          dispatch(storeAdminNotifications(notifications(state.notifications)));
        } else dispatch(storeAdminNotifications(notifications));
      },
      storeOrganizations: (
        organizations: IOrganizationModel[] | ActionDelegate<IOrganizationModel[]>,
      ) => {
        if (typeof organizations === 'function') {
          dispatch(storeAdminOrganizations(organizations(state.organizations)));
        } else dispatch(storeAdminOrganizations(organizations));
      },
      storeProducts: (products: IProductModel[] | ActionDelegate<IProductModel[]>) => {
        if (typeof products === 'function') {
          dispatch(storeAdminProducts(products(state.products)));
        } else dispatch(storeAdminProducts(products));
      },
      storeReports: (reports: IReportModel[] | ActionDelegate<IReportModel[]>) => {
        if (typeof reports === 'function') {
          dispatch(storeAdminReports(reports(state.reports)));
        } else dispatch(storeAdminReports(reports));
      },
      storeReportTemplates: (
        reportTemplates: IReportTemplateModel[] | ActionDelegate<IReportTemplateModel[]>,
      ) => {
        if (typeof reportTemplates === 'function') {
          dispatch(storeAdminReportTemplates(reportTemplates(state.reportTemplates)));
        } else dispatch(storeAdminReportTemplates(reportTemplates));
      },
      storeSeries: (series: ISeriesModel[] | ActionDelegate<ISeriesModel[]>) => {
        if (typeof series === 'function') {
          dispatch(storeAdminSeries(series(state.series)));
        } else dispatch(storeAdminSeries(series));
      },
      storeSources: (sources: ISourceModel[] | ActionDelegate<ISourceModel[]>) => {
        if (typeof sources === 'function') {
          dispatch(storeAdminSources(sources(state.sources)));
        } else dispatch(storeAdminSources(sources));
      },
      storeSystemMessages: (
        systemMessages: ISystemMessageModel[] | ActionDelegate<ISystemMessageModel[]>,
      ) => {
        if (typeof systemMessages === 'function') {
          dispatch(storeAdminSystemMessages(systemMessages(state.systemMessages)));
        } else dispatch(storeAdminSystemMessages(systemMessages));
      },
      storeTags: (tags: ITagModel[] | ActionDelegate<ITagModel[]>) => {
        if (typeof tags === 'function') {
          dispatch(storeAdminTags(tags(state.tags)));
        } else dispatch(storeAdminTags(tags));
      },
      storeTopics: (topics: ITopicModel[] | ActionDelegate<ITopicModel[]>) => {
        if (typeof topics === 'function') {
          dispatch(storeAdminTopics(topics(state.topics)));
        } else dispatch(storeAdminTopics(topics));
      },
      storeTopicScoreRules: (
        rules: ITopicScoreRuleModel[] | ActionDelegate<ITopicScoreRuleModel[]>,
      ) => {
        if (typeof rules === 'function') {
          dispatch(storeAdminTopicScoreRules(rules(state.rules)));
        } else dispatch(storeAdminTopicScoreRules(rules));
      },
      storeUserFilter: (filter: IUserListFilter | ActionDelegate<IUserListFilter>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminUserFilter(filter(state.userFilter)));
        } else dispatch(storeAdminUserFilter(filter));
      },
      storeUsers: (users: IPaged<IUserModel> | ActionDelegate<IPaged<IUserModel>>) => {
        if (typeof users === 'function') {
          dispatch(storeAdminUsers(users(state.users)));
        } else dispatch(storeAdminUsers(users));
      },
      storeWorkOrderFilter: (
        filter: IWorkOrderListFilter | ActionDelegate<IWorkOrderListFilter>,
      ) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminWorkOrderFilter(filter(state.workOrderFilter)));
        } else dispatch(storeAdminWorkOrderFilter(filter));
      },
      storeWorkOrders: (
        workOrders: IPaged<IWorkOrderModel> | ActionDelegate<IPaged<IWorkOrderModel>>,
      ) => {
        if (typeof workOrders === 'function') {
          dispatch(storeAdminWorkOrders(workOrders(state.workOrders)));
        } else dispatch(storeAdminWorkOrders(workOrders));
      },
      storeSettings: (settings: ISettingModel[] | ActionDelegate<ISettingModel[]>) => {
        if (typeof settings === 'function') {
          dispatch(storeAdminSettings(settings(state.settings)));
        } else dispatch(storeAdminSettings(settings));
      },
    }),
    [
      dispatch,
      state.actions,
      state.chartTemplates,
      state.connections,
      state.contributors,
      state.dataLocations,
      state.eveningOverviewItems,
      state.eveningOverviewSections,
      state.folders,
      state.filters,
      state.ingests,
      state.ingestTypes,
      state.licenses,
      state.ministers,
      state.notifications,
      state.organizations,
      state.products,
      state.reports,
      state.reportTemplates,
      state.rules,
      state.series,
      state.sources,
      state.systemMessages,
      state.tags,
      state.topics,
      state.userFilter,
      state.users,
      state.workOrderFilter,
      state.workOrders,
      state.settings,
    ],
  );

  return [state, controller];
};
