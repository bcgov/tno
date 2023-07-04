import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import { IWorkOrderListFilter } from 'features/admin/work-orders/interfaces/IWorkOrderListFilter';
import {
  IActionModel,
  IAlertModel,
  IConnectionModel,
  IContributorModel,
  IDataLocationModel,
  IIngestModel,
  IIngestTypeModel,
  ILicenseModel,
  INotificationModel,
  IPaged,
  IProductModel,
  IReportModel,
  IReportTemplateModel,
  ISeriesModel,
  ISourceModel,
  ITagModel,
  ITopicModel,
  ITopicScoreRuleModel,
  IUserModel,
  IWorkOrderModel,
} from 'tno-core';

export interface IAdminState {
  sources: ISourceModel[];
  connections: IConnectionModel[];
  dataLocations: IDataLocationModel[];
  products: IProductModel[];
  ingests: IIngestModel[];
  ingestTypes: IIngestTypeModel[];
  userFilter: IUserListFilter;
  users: IPaged<IUserModel>;
  topics: ITopicModel[];
  rules: ITopicScoreRuleModel[];
  tags: ITagModel[];
  alerts: IAlertModel[];
  actions: IActionModel[];
  series: ISeriesModel[];
  contributors: IContributorModel[];
  licenses: ILicenseModel[];
  workOrderFilter: IWorkOrderListFilter;
  workOrders: IPaged<IWorkOrderModel>;
  reports: IReportModel[];
  reportTemplates: IReportTemplateModel[];
  notifications: INotificationModel[];
}
