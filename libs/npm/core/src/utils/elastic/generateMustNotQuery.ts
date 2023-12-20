import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';

import { IFilterSettingsModel } from '../../hooks';
import { generateQueryValues } from './generateQueryValues';
import { removeEmptyPaths } from './removeEmptyPaths';

export const generateMustNotQuery = (
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
        must_not: generateQueryValues(settings),
      },
    },
  };

  return removeEmptyPaths(elastic);
};
