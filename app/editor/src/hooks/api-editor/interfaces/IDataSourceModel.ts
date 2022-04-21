import { DataSourceScheduleTypeName } from '../constants';
import {
  IAuditColumnsModel,
  IDataLocationModel,
  ILicenseModel,
  IMediaTypeModel,
  IScheduleModel,
  ISourceActionModel,
  ISourceMetricModel,
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
  mediaTypeId: number;
  mediaType?: IMediaTypeModel;
  licenseId: number;
  license?: ILicenseModel;
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
