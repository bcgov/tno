import {
  IAuditColumnsModel,
  IDataLocationModel,
  ILicenseModel,
  IMediaTypeModel,
  IScheduleModel,
} from '.';

export interface IDataSourceModel extends IAuditColumnsModel {
  id: number;
  name: string;
  code: string;
  description: string;
  enabled: boolean;
  dataLocationId: number;
  dataLocation?: IDataLocationModel;
  mediaTypeId: number;
  mediaType?: IMediaTypeModel;
  licenseId: number;
  license?: ILicenseModel;
  topic: string;
  inCBRA: boolean;
  inAnalysis: boolean;
  connection: any;
  lastRanOn?: Date;
  parentId?: number;
  schedules: IScheduleModel[];
}
