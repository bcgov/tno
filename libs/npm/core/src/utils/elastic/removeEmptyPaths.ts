import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';

export const removeEmptyPaths = (elastic: MsearchMultisearchBody): MsearchMultisearchBody => {
  // Remove any empty paths.
  if (elastic.query) {
    if (JSON.stringify(elastic.query.term) === '{}') delete elastic.query.term;
    if (JSON.stringify(elastic.query.multi_match) === '{}') delete elastic.query.multi_match;
    if (JSON.stringify(elastic.query.terms) === '{}') delete elastic.query.terms;
    if (elastic.query.bool) {
      if (JSON.stringify(elastic.query.bool.must) === '[]') delete elastic.query.bool.must;
      if (JSON.stringify(elastic.query.bool.must_not) === '[]') delete elastic.query.bool.must_not;
      if (JSON.stringify(elastic.query.bool.should) === '[]') delete elastic.query.bool.should;
      if (JSON.stringify(elastic.query.bool) === '{}') delete elastic.query.bool;
    }
    if (JSON.stringify(elastic.query) === '{}') delete elastic.query;
  }

  return elastic;
};
