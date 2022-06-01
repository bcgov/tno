import { IDataSourceModel, IMediaTypeModel, IPaged, IUserModel } from 'hooks/api-editor';

export interface IAdminState {
  dataSources: IDataSourceModel[];
  mediaTypes: IMediaTypeModel[];
  users: IPaged<IUserModel>;
}
