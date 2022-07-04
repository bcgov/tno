import {
  IDataSourceModel,
  IMediaTypeModel,
  IPaged,
  IUserFilter,
  IUserModel,
} from 'hooks/api-editor';

export interface IAdminState {
  dataSources: IDataSourceModel[];
  mediaTypes: IMediaTypeModel[];
  users: IPaged<IUserModel>;
  userFilter: IUserFilter;
}
