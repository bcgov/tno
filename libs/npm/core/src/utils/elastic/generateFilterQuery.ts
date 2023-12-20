import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';

import { IFilterSettingsModel } from '../../hooks';
import { generateQueryValues } from './generateQueryValues';
import { removeEmptyPaths } from './removeEmptyPaths';

/**
 * Generates an Elasticsearch query based on specified 'query'.
 * @param settings Form values that will be used to configure the elasticsearch query.
 * @param query Original query object.
 * @returns Elasticsearch query JSON.
 */
export const generateFilterQuery = (
  settings: Omit<IFilterSettingsModel, 'size' | 'searchUnpublished'>,
  query: any = {},
): MsearchMultisearchBody => {
  var elastic = { ...query };

  elastic = {
    ...elastic,
    query: {
      ...(elastic.query ?? {}),
      bool: {
        ...(elastic.query?.bool ?? {}),
        filter: generateQueryValues(settings),
      },
    },
  };

  return removeEmptyPaths(elastic);
};
