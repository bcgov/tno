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
  IWorkOrderModel,
} from 'hooks/api-editor';
import { IOptionItem } from 'tno-core';

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
  isHidden: boolean;
  isApproved: boolean;
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
  workOrders: IWorkOrderModel[];
  publishedOn: string;
  publishedOnTime: string;
  version?: number;
  // Print Content
  section: string;
  edition: string;
  byline: string;
  showOther?: boolean;
}
