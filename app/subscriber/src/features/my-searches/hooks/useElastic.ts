import React from 'react';
import { useSettings } from 'store/hooks';
import { generateMustNotQuery, generateQuery, IFilterSettingsModel } from 'tno-core';

/**
 * Hook provides helper function to generate elasticsearch query.
 * It also removes front page images from any query that isn't specifically asking for them.
 * @returns Function to generate an elasticsearch query from configuration.
 */
export const useElastic = () => {
  const { frontPageImageMediaTypeId } = useSettings();

  return React.useCallback(
    (
      filter: IFilterSettingsModel,
      query?: any,
      condition: 'must' | 'must_not' | 'filter' = 'must',
    ) => {
      if (!frontPageImageMediaTypeId) return;

      var elastic = generateQuery(filter, query, condition);
      if (frontPageImageMediaTypeId && !filter.mediaTypeIds?.includes(frontPageImageMediaTypeId)) {
        // Do not include front page images in results unless they are specifically requested.
        // filter.mediaTypeIds = [...(filter.mediaTypeIds ?? []), frontPageImageMediaTypeId];
        const mustNotQuery = generateMustNotQuery({ mediaTypeIds: [frontPageImageMediaTypeId] });
        elastic = {
          ...elastic,
          query: {
            bool: {
              ...elastic.query?.bool,
              must_not: mustNotQuery.query?.bool?.must_not,
            },
          },
        };
      }
      return elastic;
    },
    [frontPageImageMediaTypeId],
  );
};
