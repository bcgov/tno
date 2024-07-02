import {
  MsearchMultisearchBody,
  QueryDslQueryContainer,
} from '@elastic/elasticsearch/lib/api/types';

const removeEmptyShouldClauses = (
  query: QueryDslQueryContainer | QueryDslQueryContainer[] | undefined,
) => {
  if (!query) return;

  if (Array.isArray(query)) {
    // Handle case where query is an array of QueryDslQueryContainer
    for (const q of query) {
      if (q.bool?.should && Array.isArray(q.bool.should) && q.bool.should.length === 0) {
        delete q.bool.minimum_should_match;
        delete q.bool.should;
        delete q.bool;
      }
    }
  } else {
    // Handle case where query is a single QueryDslQueryContainer
    if (query.bool?.should && Array.isArray(query.bool.should) && query.bool.should.length === 0) {
      delete query.bool.minimum_should_match;
      delete query.bool.should;
      delete query.bool;
    }
  }
};

const removeEmptyObjects = (query: QueryDslQueryContainer[] | undefined) => {
  if (!query) return;

  // Filter out empty objects
  for (let i = 0; i < query.length; i++) {
    const q = query[i];
    if (Object.keys(q).length === 0) {
      query.splice(i, 1);
      i--; // Adjust index after removal
    }
  }
};

export const removeEmptyPaths = (elastic: MsearchMultisearchBody): MsearchMultisearchBody => {
  // Check if elastic.query exists and handle both single and array cases
  if (elastic.query) {
    if (JSON.stringify(elastic.query.term) === '{}') delete elastic.query.term;
    if (JSON.stringify(elastic.query.multi_match) === '{}') delete elastic.query.multi_match;
    if (JSON.stringify(elastic.query.terms) === '{}') delete elastic.query.terms;
    if (elastic.query.bool) {
      if (JSON.stringify(elastic.query.bool.must) === '[]') delete elastic.query.bool.must;
      if (JSON.stringify(elastic.query.bool.must_not) === '[]') delete elastic.query.bool.must_not;
      if (JSON.stringify(elastic.query.bool.should) === '[]') delete elastic.query.bool.should;
      if (JSON.stringify(elastic.query.bool) === '{}') delete elastic.query.bool;

      const must = elastic.query.bool?.must as QueryDslQueryContainer | QueryDslQueryContainer[];
      const mustNot = elastic.query.bool?.must_not as
        | QueryDslQueryContainer
        | QueryDslQueryContainer[];

      removeEmptyShouldClauses(must);
      removeEmptyShouldClauses(mustNot);

      // Remove empty objects from must and must_not
      removeEmptyObjects(Array.isArray(must) ? must : must ? [must] : undefined);
      removeEmptyObjects(Array.isArray(mustNot) ? mustNot : mustNot ? [mustNot] : undefined);
    }
  }

  return elastic;
};
