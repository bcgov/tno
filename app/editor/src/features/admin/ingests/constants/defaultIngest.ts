import { IIngestModel, ScheduleTypeName } from 'hooks/api-editor';

export const defaultIngest: IIngestModel = {
  id: 0,
  name: '',
  topic: '',
  description: '',
  isEnabled: false,
  sourceId: 0,
  productId: 0,
  ingestTypeId: 0,
  scheduleType: ScheduleTypeName.None,
  configuration: {
    post: true,
    import: true,
  },
  sourceConnectionId: 0,
  destinationConnectionId: 0,
  retryLimit: 0,
  failedAttempts: 0,
  lastRanOn: undefined,
  schedules: [],
  dataLocations: [],
};
