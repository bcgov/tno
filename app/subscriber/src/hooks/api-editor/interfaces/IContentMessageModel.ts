import { ContentStatusName, ContentTypeName } from 'hooks';

export interface IContentMessageModel {
  id: number;
  contentType: ContentTypeName;
  status: ContentStatusName;
  ownerId?: number;
  headline: string;
}
