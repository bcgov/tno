import { ContentStatus, IMediaTypeModel, IUserModel } from '..';

export interface IContentModel {
  id: number;
  status: ContentStatus;
  headline: string;
  page: string;
  ownerId: number;
  owner?: IUserModel;
  source: string;
  section: string;
  mediaTypeId: number;
  mediaType: IMediaTypeModel;
  date: Date;
  use: boolean;
}
