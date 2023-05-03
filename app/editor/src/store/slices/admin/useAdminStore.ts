import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import { IWorkOrderListFilter } from 'features/admin/work-orders/interfaces/IWorkOrderListFilter';
import React from 'react';
import { ActionDelegate, useAppDispatch, useAppSelector } from 'store';
import {
  IActionModel,
  IAlertModel,
  IConnectionModel,
  IDataLocationModel,
  IIngestModel,
  IIngestTypeModel,
  ILicenseModel,
  IPaged,
  IProductModel,
  ISeriesModel,
  ISourceModel,
  ITagModel,
  ITopicModel,
  ITopicScoreRuleModel,
  IUserModel,
  IWorkOrderModel,
} from 'tno-core';

import {
  storeAdminActions,
  storeAdminAlerts,
  storeAdminConnections,
  storeAdminDataLocations,
  storeAdminIngests,
  storeAdminIngestTypes,
  storeAdminLicenses,
  storeAdminProducts,
  storeAdminSeries,
  storeAdminSources,
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
  storeSources: (sources: ISourceModel[] | ActionDelegate<ISourceModel[]>) => void;
  storeConnections: (connections: IConnectionModel[] | ActionDelegate<IConnectionModel[]>) => void;
  storeDataLocations: (
    dataLocations: IDataLocationModel[] | ActionDelegate<IDataLocationModel[]>,
  ) => void;
  storeProducts: (products: IProductModel[] | ActionDelegate<IProductModel[]>) => void;
  storeLicenses: (licenses: ILicenseModel[] | ActionDelegate<ILicenseModel[]>) => void;
  storeIngests: (ingests: IIngestModel[] | ActionDelegate<IIngestModel[]>) => void;
  storeIngestTypes: (ingestTypes: IIngestTypeModel[] | ActionDelegate<IIngestTypeModel[]>) => void;
  storeTopics: (topics: ITopicModel[] | ActionDelegate<ITopicModel[]>) => void;
  storeTopicScoreRules: (
    rules: ITopicScoreRuleModel[] | ActionDelegate<ITopicScoreRuleModel[]>,
  ) => void;
  storeUserFilter: (filter: IUserListFilter | ActionDelegate<IUserListFilter>) => void;
  storeUsers: (users: IPaged<IUserModel> | ActionDelegate<IPaged<IUserModel>>) => void;
  storeTags: (tags: ITagModel[] | ActionDelegate<ITagModel[]>) => void;
  storeAlerts: (alerts: IAlertModel[] | ActionDelegate<IAlertModel[]>) => void;
  storeActions: (actions: IActionModel[] | ActionDelegate<IActionModel[]>) => void;
  storeSeries: (series: ISeriesModel[] | ActionDelegate<ISeriesModel[]>) => void;
  storeWorkOrderFilter: (
    filter: IWorkOrderListFilter | ActionDelegate<IWorkOrderListFilter>,
  ) => void;
  storeWorkOrders: (
    users: IPaged<IWorkOrderModel> | ActionDelegate<IPaged<IWorkOrderModel>>,
  ) => void;
}

export const useAdminStore = (props?: IAdminProps): [IAdminState, IAdminStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.admin);

  const controller = React.useMemo(
    () => ({
      storeSources: (sources: ISourceModel[] | ActionDelegate<ISourceModel[]>) => {
        if (typeof sources === 'function') {
          dispatch(storeAdminSources(sources(state.sources)));
        } else dispatch(storeAdminSources(sources));
      },
      storeConnections: (connections: IConnectionModel[] | ActionDelegate<IConnectionModel[]>) => {
        if (typeof connections === 'function') {
          dispatch(storeAdminConnections(connections(state.connections)));
        } else dispatch(storeAdminConnections(connections));
      },
      storeDataLocations: (
        dataLocations: IDataLocationModel[] | ActionDelegate<IDataLocationModel[]>,
      ) => {
        if (typeof dataLocations === 'function') {
          dispatch(storeAdminDataLocations(dataLocations(state.dataLocations)));
        } else dispatch(storeAdminDataLocations(dataLocations));
      },
      storeProducts: (products: IProductModel[] | ActionDelegate<IProductModel[]>) => {
        if (typeof products === 'function') {
          dispatch(storeAdminProducts(products(state.products)));
        } else dispatch(storeAdminProducts(products));
      },
      storeLicenses: (licenses: ILicenseModel[] | ActionDelegate<ILicenseModel[]>) => {
        if (typeof licenses === 'function') {
          dispatch(storeAdminLicenses(licenses(state.licenses)));
        } else dispatch(storeAdminLicenses(licenses));
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
      storeTags: (tags: ITagModel[] | ActionDelegate<ITagModel[]>) => {
        if (typeof tags === 'function') {
          dispatch(storeAdminTags(tags(state.tags)));
        } else dispatch(storeAdminTags(tags));
      },
      storeAlerts: (alerts: IAlertModel[] | ActionDelegate<IAlertModel[]>) => {
        if (typeof alerts === 'function') {
          dispatch(storeAdminAlerts(alerts(state.alerts)));
        } else dispatch(storeAdminAlerts(alerts));
      },
      storeActions: (actions: IActionModel[] | ActionDelegate<IActionModel[]>) => {
        if (typeof actions === 'function') {
          dispatch(storeAdminActions(actions(state.actions)));
        } else dispatch(storeAdminActions(actions));
      },
      storeSeries: (series: ISeriesModel[] | ActionDelegate<ISeriesModel[]>) => {
        if (typeof series === 'function') {
          dispatch(storeAdminSeries(series(state.series)));
        } else dispatch(storeAdminSeries(series));
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
    }),
    [
      dispatch,
      state.actions,
      state.connections,
      state.dataLocations,
      state.ingestTypes,
      state.ingests,
      state.licenses,
      state.products,
      state.rules,
      state.series,
      state.sources,
      state.tags,
      state.topics,
      state.userFilter,
      state.users,
      state.workOrderFilter,
      state.workOrders,
    ],
  );

  return [state, controller];
};
