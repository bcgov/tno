import { type estypes } from '@elastic/elasticsearch';

// The Elasticsearch request/response types the apps use. The client exposes them only through its
// estypes namespace (its package exports do not allow importing lib/api/types directly).
export type IndicesValidateQueryResponse = estypes.IndicesValidateQueryResponse;
export type KnnSearchResponse<T = unknown> = estypes.KnnSearchResponse<T>;
export type MsearchMultisearchBody = estypes.MsearchMultisearchBody;
export type QueryDslQueryContainer = estypes.QueryDslQueryContainer;
export type SearchTotalHits = estypes.SearchTotalHits;
