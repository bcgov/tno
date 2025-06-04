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

  elastic = {
    ...elastic,
    query: {
      ...(elastic.query ?? {}),
      bool: {
        ...(elastic.query?.bool ?? {}),
        must: generateQueryValues(settings),
        filter: filter ?? elastic.query?.filter,
      },
    },
  };

  return removeEmptyPaths(elastic);
};
