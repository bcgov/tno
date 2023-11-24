import { ScheduleTypeName } from '../constants';
import {
  IAuditColumnsModel,
  IConnectionModel,
  IDataLocationModel,
  IIngestTypeModel,
  IMediaTypeModel,
  IScheduleModel,
  ISourceModel,
} from '.';

export interface IIngestModel extends IAuditColumnsModel {
  id: number;
  name: string;
  topic: string;
  description: string;
  isEnabled: boolean;

  sourceId: number;
  source?: ISourceModel;
  ingestTypeId: number;
  ingestType?: IIngestTypeModel;
  mediaTypeId: number;
  mediaType?: IMediaTypeModel;
  scheduleType: ScheduleTypeName;
  configuration: any;
  retryLimit: number;
  sourceConnectionId: number;
  sourceConnection?: IConnectionModel;
  destinationConnectionId: number;
  destinationConnection?: IConnectionModel;
  schedules: IScheduleModel[];
  dataLocations: IDataLocationModel[];

  // State properties
  lastRanOn?: string;
  failedAttempts: number;
}
