import {
  IActionModel,
  ICategoryModel,
  IDataSourceModel,
  IMediaTypeModel,
  IPaged,
  ISeriesModel,
  ITagModel,
  IUserFilter,
  IUserModel,
} from 'hooks/api-editor';
import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';

export interface IAdminState {
  dataSources: IDataSourceModel[];
  mediaTypes: IMediaTypeModel[];
  users: IPaged<IUserModel>;
  categories: ICategoryModel[];
  tags: ITagModel[];
  actions: IActionModel[];
  series: ISeriesModel[];
  userFilter: IUserListFilter;
}
