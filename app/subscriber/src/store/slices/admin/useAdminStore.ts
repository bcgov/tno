import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import { IWorkOrderListFilter } from 'features/admin/work-orders/interfaces/IWorkOrderListFilter';
import React from 'react';
import { ActionDelegate, useAppDispatch, useAppSelector } from 'store';
import {
  IActionModel,
  IConnectionModel,
  IDataLocationModel,
  IIngestModel,
  IIngestTypeModel,
  ILicenseModel,
  IMinisterModel,
  IPaged,
  IProductModel,
  ISeriesModel,
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
  storeAdminConnections,
  storeAdminDataLocations,
  storeAdminIngests,
  storeAdminIngestTypes,
  storeAdminLicenses,
  storeAdminMinisters,
  storeAdminProducts,
  storeAdminSeries,
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
  storeActions: (actions: IActionModel[]) => void;
  storeConnections: (connections: IConnectionModel[]) => void;
  storeDataLocations: (dataLocations: IDataLocationModel[]) => void;
  storeIngests: (ingests: IIngestModel[]) => void;
  storeIngestTypes: (ingestTypes: IIngestTypeModel[]) => void;
  storeLicenses: (licenses: ILicenseModel[]) => void;
  storeMinisters: (ministers: IMinisterModel[]) => void;
  storeProducts: (products: IProductModel[]) => void;
  storeSeries: (series: ISeriesModel[]) => void;
  storeSources: (sources: ISourceModel[]) => void;
  storeSystemMessages: (
    systemMessages: ISystemMessageModel[] | ActionDelegate<ISystemMessageModel[]>,
  ) => void;
  storeTags: (tags: ITagModel[]) => void;
  storeTopics: (topics: ITopicModel[]) => void;
  storeTopicScoreRules: (rules: ITopicScoreRuleModel[]) => void;
  storeUserFilter: (filter: IUserListFilter) => void;
  storeUsers: (users: IPaged<IUserModel>) => void;
  storeWorkOrderFilter: (filter: IWorkOrderListFilter) => void;
  storeWorkOrders: (users: IPaged<IWorkOrderModel>) => void;
}

export const useAdminStore = (props?: IAdminProps): [IAdminState, IAdminStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.admin);

  const controller = React.useMemo(
    () => ({
      storeActions: (actions: IActionModel[]) => {
        dispatch(storeAdminActions(actions));
      },
      storeConnections: (connections: IConnectionModel[]) => {
        dispatch(storeAdminConnections(connections));
      },
      storeDataLocations: (dataLocations: IDataLocationModel[]) => {
        dispatch(storeAdminDataLocations(dataLocations));
      },
      storeIngests: (ingests: IIngestModel[]) => {
        dispatch(storeAdminIngests(ingests));
      },
      storeIngestTypes: (ingestTypes: IIngestTypeModel[]) => {
        dispatch(storeAdminIngestTypes(ingestTypes));
      },
      storeLicenses: (licenses: ILicenseModel[]) => {
        dispatch(storeAdminLicenses(licenses));
      },
      storeMinisters: (ministers: IMinisterModel[]) => {
        dispatch(storeAdminMinisters(ministers));
      },
      storeProducts: (products: IProductModel[]) => {
        dispatch(storeAdminProducts(products));
      },
      storeSeries: (series: ISeriesModel[]) => {
        dispatch(storeAdminSeries(series));
      },
      storeSources: (sources: ISourceModel[]) => {
        dispatch(storeAdminSources(sources));
      },
      storeSystemMessages: (
        systemMessages: ISystemMessageModel[] | ActionDelegate<ISystemMessageModel[]>,
      ) => {
        if (typeof systemMessages === 'function') {
          dispatch(storeAdminSystemMessages(systemMessages(state.systemMessages)));
        } else dispatch(storeAdminSystemMessages(systemMessages));
      },
      storeTags: (tags: ITagModel[]) => {
        dispatch(storeAdminTags(tags));
      },
      storeTopics: (topics: ITopicModel[]) => {
        dispatch(storeAdminTopics(topics));
      },
      storeTopicScoreRules: (rules: ITopicScoreRuleModel[]) => {
        dispatch(storeAdminTopicScoreRules(rules));
      },
      storeUserFilter: (filter: IUserListFilter) => {
        dispatch(storeAdminUserFilter(filter));
      },
      storeUsers: (users: IPaged<IUserModel>) => {
        dispatch(storeAdminUsers(users));
      },
      storeWorkOrderFilter: (filter: IWorkOrderListFilter) => {
        dispatch(storeAdminWorkOrderFilter(filter));
      },
      storeWorkOrders: (workOrders: IPaged<IWorkOrderModel>) => {
        dispatch(storeAdminWorkOrders(workOrders));
      },
    }),
    [dispatch, state.systemMessages],
  );

  return [state, controller];
};
