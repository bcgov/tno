import {
  ContentStatusName,
  ContentTypeName,
  IAuditColumnsModel,
  ILicenseModel,
  IMediaTypeModel,
  ISeriesModel,
  IUserModel,
} from '..';
import {
  IContentActionModel,
  IContentLabelModel,
  IContentLinkModel,
  IContentTagModel,
  IContentTonePoolModel,
  IContentTopicModel,
  IContentVersionModel,
  IContributorModel,
  IFileReferenceModel,
  IQuoteModel,
  ISourceModel,
  ITimeTrackingModel,
  IUserContentNotificationModel,
} from '.';

export interface IContentModel extends IAuditColumnsModel {
  id: number;
  status: ContentStatusName;
  contentType: ContentTypeName;
  licenseId: number;
  license?: ILicenseModel;
  sourceId?: number;
  source?: ISourceModel;
  otherSource: string;
  mediaTypeId: number;
  mediaType?: IMediaTypeModel;
  seriesId?: number;
  series?: ISeriesModel;
  contributorId?: number;
  contributor?: IContributorModel;
  otherSeries?: string;
  ownerId?: number;
  owner?: IUserModel;
  headline: string;
  byline: string;
  uid?: string;
  edition: string;
  section: string;
  page: string;
  postedOn?: string;
  publishedOn: string;
  summary: string;
  body: string;
  sourceUrl?: string;
  isHidden: boolean;
  isApproved: boolean;
  isPrivate: boolean;
  actions: IContentActionModel[];
  tags: IContentTagModel[];
  labels: IContentLabelModel[];
  topics: IContentTopicModel[];
  tonePools: IContentTonePoolModel[];
  timeTrackings: ITimeTrackingModel[];
  fileReferences: IFileReferenceModel[];
  links: IContentLinkModel[];
  quotes: IQuoteModel[];
  userNotifications: IUserContentNotificationModel[];
  versions: Record<number, IContentVersionModel>;

  // React-Table Properties
  // TODO: Should not be part of the API interface.
  isSelected?: boolean;
}
