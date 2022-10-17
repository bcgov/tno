import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import { IWorkOrderListFilter } from 'features/admin/work-orders/interfaces/IWorkOrderListFilter';
import {
  IActionModel,
  ICategoryModel,
  IConnectionModel,
  IIngestModel,
  IIngestTypeModel,
  ILicenseModel,
  IPaged,
  IProductModel,
  ISeriesModel,
  ISourceModel,
  ITagModel,
  IUserModel,
  IWorkOrderModel,
} from 'hooks/api-editor';

export interface IAdminState {
  sources: ISourceModel[];
  connections: IConnectionModel[];
  products: IProductModel[];
  ingests: IIngestModel[];
  ingestTypes: IIngestTypeModel[];
  userFilter: IUserListFilter;
  users: IPaged<IUserModel>;
  categories: ICategoryModel[];
  tags: ITagModel[];
  actions: IActionModel[];
  series: ISeriesModel[];
  licenses: ILicenseModel[];
  workOrderFilter: IWorkOrderListFilter;
  workOrders: IPaged<IWorkOrderModel>;
}
