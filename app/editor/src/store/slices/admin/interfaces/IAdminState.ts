import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import {
  IActionModel,
  ICategoryModel,
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
  IUserModel,
} from 'hooks/api-editor';

export interface IAdminState {
  sources: ISourceModel[];
  connections: IConnectionModel[];
  dataLocations: IDataLocationModel[];
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
}
