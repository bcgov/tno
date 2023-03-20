import { ContentStatusName, ContentTypeName } from '../constants';

export interface IContentMessageModel {
  id: number;
  contentType: ContentTypeName;
  status: ContentStatusName;
  ownerId?: number;
  headline: string;
}
