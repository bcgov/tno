import { ScheduleTypeName } from '../constants';
import {
  IAuditColumnsModel,
  IConnectionModel,
  IDataLocationModel,
  IIngestTypeModel,
  IProductModel,
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
  productId: number;
  product?: IProductModel;
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
  lastRanOn?: Date;
  failedAttempts: number;
}
