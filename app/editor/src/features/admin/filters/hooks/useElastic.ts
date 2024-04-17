import React from 'react';
import { useSettings } from 'store/hooks';
import { generateMustNotQuery, generateQuery, IFilterSettingsModel } from 'tno-core';

/**
 * Hook provides helper function to generate elasticsearch query.
 * It also removes front page images from any query that isn't specifically asking for them.
 * @returns Function to generate an elasticsearch query from configuration.
 */
export const useElastic = () => {
  const { frontPageImagesMediaTypeId } = useSettings();

  return React.useCallback(
    (
      filter: IFilterSettingsModel,
      query?: any,
      condition: 'must' | 'must_not' | 'filter' = 'must',
    ) => {
      var elastic = generateQuery(filter, query, condition);
      if (
        frontPageImagesMediaTypeId &&
        !filter.mediaTypeIds?.includes(frontPageImagesMediaTypeId)
      ) {
        // Do not include front page images in results unless they are specifically requested.
        elastic = generateMustNotQuery({ mediaTypeIds: [frontPageImagesMediaTypeId] }, elastic);
      } else {
        elastic = generateMustNotQuery({}, elastic);
      }
      return elastic;
    },
    [frontPageImagesMediaTypeId],
  );
};
