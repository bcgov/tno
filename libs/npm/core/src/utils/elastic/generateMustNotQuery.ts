import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';

import { IFilterSettingsModel } from '../../hooks';
import { generatePublishedOnQuery } from './generatePublishedOnQuery';
import { generateQueryValues } from './generateQueryValues';
import { removeEmptyPaths } from './removeEmptyPaths';

export const generateMustNotQuery = (
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
        must_not: generateQueryValues(settings),
        filter: filter ?? elastic.query?.filter,
      },
    },
  };

  return removeEmptyPaths(elastic);
};
