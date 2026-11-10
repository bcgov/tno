import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';

import { IFilterSettingsModel } from '../../hooks';
import { generatePublishedOnQuery } from './generatePublishedOnQuery';
import { generateQueryValues } from './generateQueryValues';
import { removeEmptyPaths } from './removeEmptyPaths';

/**
 * Generates an Elasticsearch query based on specified 'query'.
 * @param settings Form values that will be used to configure the elasticsearch query.
 * @param query Original query object.
 * @returns Elasticsearch query JSON.
 */
export const generateMustQuery = (
  settings: Omit<IFilterSettingsModel, 'size' | 'searchUnpublished'>,
  query: any = {},
): MsearchMultisearchBody => {
  var elastic = { ...query };

  const publishedOn = generatePublishedOnQuery(settings);
  const filter = publishedOn ? [publishedOn] : undefined;

  // A 'bool' clause replaces any prior 'match_all' placeholder; a query object must
  // contain exactly one root clause or Elasticsearch rejects it.
  const { match_all, ...priorQuery } = elastic.query ?? {};
  elastic = {
    ...elastic,
    query: {
      ...priorQuery,
      bool: {
        ...(elastic.query?.bool ?? {}),
        must: generateQueryValues(settings),
        filter: filter ?? elastic.query?.filter,
      },
    },
  };

  return removeEmptyPaths(elastic);
};
