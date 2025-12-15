import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';

import { IFilterSettingsModel } from '../../hooks';
import { generateFilterQuery } from './generateFilterQuery';
import { generateMustNotQuery } from './generateMustNotQuery';
import { generateMustQuery } from './generateMustQuery';

/**
 * Generates an Elasticsearch query based on specified 'query'.
 * @param settings Form values that will be used to configure the elasticsearch query.
 * @param query Original query object.
 * @returns Elasticsearch query JSON.
 */
export const generateQuery = (
  settings: IFilterSettingsModel,
  query: any = {},
  condition: 'must' | 'must_not' | 'filter' = 'must',
): MsearchMultisearchBody => {
  var elastic = query;
  if (condition === 'must') elastic = generateMustQuery(settings, query);
  else if (condition === 'filter') elastic = generateFilterQuery(settings, query);
  else if (condition === 'must_not') elastic = generateMustNotQuery(settings, query);

  if (!elastic.query || Object.keys(elastic.query).length === 0) elastic.query = { match_all: {} };

  if (settings.size) elastic = { ...elastic, size: settings.size };
  if (settings.from) elastic = { ...elastic, from: settings.from };

  elastic = { ...elastic, sort: settings.sort ? settings.sort : [{ publishedOn: 'desc' }] };

  return elastic;
};
