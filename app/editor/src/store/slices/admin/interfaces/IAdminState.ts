import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import {
  IActionModel,
  ICategoryModel,
  IConnectionModel,
  IIngestModel,
  ILicenseModel,
  IMediaTypeModel,
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
  mediaTypes: IMediaTypeModel[];
  users: IPaged<IUserModel>;
  categories: ICategoryModel[];
  tags: ITagModel[];
  actions: IActionModel[];
  series: ISeriesModel[];
  licenses: ILicenseModel[];
  userFilter: IUserListFilter;
}
