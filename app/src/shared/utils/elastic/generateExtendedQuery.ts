import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';

import { IFilterExtendedQuery } from '../../hooks';

/** The name of the clause that carries a filter's extended query. */
export const EXTENDED_QUERY_NAME = 'extendedQuery';

/**
 * Whether the clause is the one that carries a filter's extended query.
 * @param clause A query clause.
 * @returns True when it is the named extended query clause.
 */
const isExtendedQueryClause = (clause: any) => clause?.bool?._name === EXTENDED_QUERY_NAME;

/**
 * Merges a filter's extended query into the query as a single named 'bool' clause in the filter
 * context, replacing the clause from an earlier merge. The query is rebuilt from settings on
 * every change, and an earlier filter clause may survive the rebuild, so the merge must be
 * idempotent.
 * @param elastic The query generated from the settings.
 * @param extendedQuery The bool clauses the settings cannot express.
 * @returns Elasticsearch query JSON.
 */
export const generateExtendedQuery = (
  elastic: MsearchMultisearchBody,
  extendedQuery?: IFilterExtendedQuery,
): MsearchMultisearchBody => {
  const query: any = elastic.query ?? {};
  const bool: any = query.bool;
  const prior = bool?.filter;
  const priorFilter = prior === undefined ? [] : Array.isArray(prior) ? prior : [prior];
  const filter = priorFilter.filter((clause: any) => !isExtendedQueryClause(clause));
  const hasClauses =
    !!extendedQuery &&
    ['must', 'filter', 'must_not', 'should'].some((key) => (extendedQuery as any)[key]?.length > 0);

  if (!hasClauses) {
    if (filter.length === priorFilter.length) return elastic;
    const { filter: _, ...rest } = bool;
    return {
      ...elastic,
      query: { ...query, bool: filter.length ? { ...rest, filter } : rest },
    };
  }

  // A 'bool' clause replaces any prior 'match_all' placeholder; a query object must contain
  // exactly one root clause or Elasticsearch rejects it.
  const { match_all, ...priorQuery } = query;
  return {
    ...elastic,
    query: {
      ...priorQuery,
      bool: {
        ...(bool ?? {}),
        filter: [...filter, { bool: { ...extendedQuery, _name: EXTENDED_QUERY_NAME } }],
      },
    },
  };
};
