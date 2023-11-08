import {
  ContentStatusName,
  ContentTypeName,
  IContentActionModel,
  IContentLabelModel,
  IContentLinkModel,
  IContentTagModel,
  IContentTonePoolModel,
  IContentTopicModel,
  IFileReferenceModel,
  IMediaTypeModel,
  ITimeTrackingModel,
  IUserModel,
  IWorkOrderModel,
} from 'tno-core';

export interface IContentForm {
  id: number;
  uid: string;
  sourceUrl: string;
  headline: string;
  status: ContentStatusName;
  contentType: ContentTypeName;
  ownerId: number | '';
  owner?: IUserModel;
  mediaTypeId: number;
  mediaType?: IMediaTypeModel;
  licenseId: number;
  sourceId?: number;
  otherSource: string;
  tempSource: string;
  seriesId?: number;
  contributorId?: number;
  otherSeries: string;
  page: string;
  summary: string;
  body: string;
  isHidden: boolean;
  isApproved: boolean;
  actions: IContentActionModel[];
  topics: IContentTopicModel[];
  tags: IContentTagModel[];
  labels: IContentLabelModel[];
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

  createdOn?: string | Date;
}
