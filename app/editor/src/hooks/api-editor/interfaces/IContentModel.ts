import {
  ContentStatus,
  IAuditColumnsModel,
  IContentTypeModel,
  ILicenseModel,
  IMediaTypeModel,
  ISeriesModel,
  IUserModel,
  WorkflowStatus,
} from '..';
import { IPrintContentModel } from '.';
import { IActionModel } from './IActionModel';
import { ITimeTrackingModel } from './ITimeTrackingModel';

export interface IContentModel extends IAuditColumnsModel {
  id: number;
  printContent?: IPrintContentModel;
  status: ContentStatus;
  workflowStatus: WorkflowStatus;
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
  ownerId: number;
  owner?: IUserModel;
  headline: string;
  uid?: string;
  page: string;
  publishedOn: Date;
  summary: string;
  transcription?: string;
  sourceUrl?: string;
  timeTrackings?: ITimeTrackingModel[];
  actions?: IActionModel[];
}
