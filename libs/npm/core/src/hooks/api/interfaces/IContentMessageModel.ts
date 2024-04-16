import { ContentStatusName, ContentTypeName } from '../constants';
import { IContentActionModel, IContentTonePoolModel, IFileReferenceModel } from '.';

export interface IContentMessageModel {
  id: number;
  contentType: ContentTypeName;
  status: ContentStatusName;
  sourceId: number;
  otherSource: string;
  licenseId: number;
  seriesId?: number;
  mediaTypeId: number;
  ownerId?: number;
  uid: string;
  headline: string;
  byline: string;
  edition: string;
  section: string;
  page: string;
  isApproved: boolean;
  postedOn?: string;
  publishedOn: string;
  fileReferences: IFileReferenceModel[];
  tonePools: IContentTonePoolModel[];
  actions: IContentActionModel[];
  version?: number;
  reason?: string;
}
