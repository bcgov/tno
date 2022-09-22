import { IOptionItem } from 'components/form';
import {
  ContentStatusName,
  ContentTypeName,
  IContentActionModel,
  IContentCategoryModel,
  IContentLabelModel,
  IContentLinkModel,
  IContentTagModel,
  IContentTonePoolModel,
  IFileReferenceModel,
  ITimeTrackingModel,
} from 'hooks/api-editor';

export interface IContentForm {
  id: number;
  uid: string;
  sourceUrl: string;
  headline: string;
  status: ContentStatusName;
  contentType: ContentTypeName;
  ownerId: number | '';
  productId: number;
  licenseId: number;
  sourceId?: number;
  otherSource: string;
  tempSource: string;
  seriesId?: number;
  otherSeries: string;
  page: string;
  summary: string;
  body: string;
  actions: IContentActionModel[];
  categories: IContentCategoryModel[];
  tags: IContentTagModel[];
  labels: IContentLabelModel[];
  tone: number | '';
  tonePool?: IOptionItem;
  tonePools: IContentTonePoolModel[];
  timeTrackings: ITimeTrackingModel[];
  fileReferences: IFileReferenceModel[];
  file?: File | null;
  links: IContentLinkModel[];
  publishedOn: string;
  version?: number;
  // Print Content
  section: string;
  edition: string;
  storyType: string;
  byline: string;
  showOther?: boolean;
}
