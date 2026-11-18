import { IMediaTypeModel } from './IMediaTypeModel';

export interface IUserMediaTypeModel {
  userId: number;
  mediaTypeId: number;
  mediaType?: IMediaTypeModel;
}
