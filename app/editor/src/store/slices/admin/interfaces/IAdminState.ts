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

export interface IAdminState {
  dataSources: IDataSourceModel[];
  mediaTypes: IMediaTypeModel[];
  users: IPaged<IUserModel>;
  categories: ICategoryModel[];
  tags: ITagModel[];
  actions: IActionModel[];
  series: ISeriesModel[];
  userFilter: IUserFilter;
}
