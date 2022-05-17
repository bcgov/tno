import {
  ContentStatusName,
  IAuditColumnsModel,
  IContentTypeModel,
  ILicenseModel,
  IMediaTypeModel,
  ISeriesModel,
  IUserModel,
  WorkflowStatusName,
} from '..';
import {
  IContentActionModel,
  IContentCategoryModel,
  IContentLinkModel,
  IContentTagModel,
  IContentTonePoolModel,
  IFileReferenceModel,
  IPrintContentModel,
  ITimeTrackingModel,
} from '.';

export interface IContentModel extends IAuditColumnsModel {
  id: number;
  printContent?: IPrintContentModel;
  status: ContentStatusName;
  workflowStatus: WorkflowStatusName;
  contentTypeId: number;
  contentType?: IContentTypeModel;
  mediaTypeId: number;
  mediaType?: IMediaTypeModel;
  licenseId: number;
  license?: ILicenseModel;
  dataSourceId?: number;
  dataSource?: ISeriesModel;
  source: string;
  seriesId?: number;
  series?: ISeriesModel;
  otherSeries?: string;
  ownerId?: number;
  owner?: IUserModel;
  headline: string;
  uid?: string;
  page: string;
  publishedOn: Date;
  summary: string;
  transcription?: string;
  sourceUrl?: string;
  actions?: IContentActionModel[];
  tags?: IContentTagModel[];
  categories?: IContentCategoryModel[];
  tonePools?: IContentTonePoolModel[];
  timeTrackings?: ITimeTrackingModel[];
  fileReferences?: IFileReferenceModel[];
  links?: IContentLinkModel[];
}
