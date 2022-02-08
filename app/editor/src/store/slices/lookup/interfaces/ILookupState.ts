import { IContentTypeModel, IMediaTypeModel, IUserModel } from 'hooks';

export interface ILookupState {
  contentTypes: IContentTypeModel[];
  mediaTypes: IMediaTypeModel[];
  users: IUserModel[];
}
