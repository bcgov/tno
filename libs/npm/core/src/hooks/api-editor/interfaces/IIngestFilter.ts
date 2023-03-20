import { ISortPageFilter } from '.';

export interface IIngestFilter extends ISortPageFilter {
  name?: string;
  topic?: string;
  ingestTypeId?: number[];
  sourceId?: number;
  productId?: number;
  serviceType?: string;
  sourceConnectionId?: number;
  destinationConnectionId?: number;
  isEnabled?: boolean;
}
