import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
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
} from 'hooks/api-editor';

export interface IAdminState {
  sources: ISourceModel[];
  connections: IConnectionModel[];
  products: IProductModel[];
  ingests: IIngestModel[];
  ingestTypes: IIngestTypeModel[];
  users: IPaged<IUserModel>;
  categories: ICategoryModel[];
  tags: ITagModel[];
  actions: IActionModel[];
  series: ISeriesModel[];
  licenses: ILicenseModel[];
  userFilter: IUserListFilter;
}
