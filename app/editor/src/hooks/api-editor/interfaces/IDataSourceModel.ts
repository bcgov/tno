import { DataSourceScheduleTypeName } from '../constants';
import {
  IAuditColumnsModel,
  IContentTypeModel,
  IDataLocationModel,
  ILicenseModel,
  IMediaTypeModel,
  IScheduleModel,
  ISourceActionModel,
  ISourceMetricModel,
  IUserModel,
} from '.';

export interface IDataSourceModel extends IAuditColumnsModel {
  id: number;
  name: string;
  code: string;
  shortName: string;
  description: string;
  isEnabled: boolean;
  dataLocationId: number;
  dataLocation?: IDataLocationModel;
  contentTypeId?: number;
  contentType?: IContentTypeModel;
  mediaTypeId: number;
  mediaType?: IMediaTypeModel;
  licenseId: number;
  license?: ILicenseModel;
  ownerId?: number;
  owner?: IUserModel;
  scheduleType: DataSourceScheduleTypeName;
  topic: string;
  connection: any;
  lastRanOn?: Date;
  retryLimit: number;
  failedAttempts: number;
  parentId?: number;
  actions: ISourceActionModel[];
  metrics: ISourceMetricModel[];
  schedules: IScheduleModel[];
}
