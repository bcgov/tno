/**
 * Raw Elasticsearch bool clauses for filter conditions the settings cannot express.
 * They are merged into the generated query as a single named 'bool' filter clause.
 */
export interface IFilterExtendedQuery {
  must?: any[];
  filter?: any[];
  must_not?: any[];
  should?: any[];
  minimum_should_match?: number | string;
}
