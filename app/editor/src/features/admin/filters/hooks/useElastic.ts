import React from 'react';
import { useSettings } from 'store/hooks';
import { generateMustNotQuery, generateQuery, type IFilterSettingsModel } from 'tno-core';

/**
 * Hook provides helper function to generate elasticsearch query.
 * It also removes front page images from any query that isn't specifically asking for them.
 * @returns Function to generate an elasticsearch query from configuration.
 */
export const useElastic = () => {
  const { frontpageImageMediaTypeId } = useSettings();

  return React.useCallback(
    (
      filter: IFilterSettingsModel,
      query?: any,
      condition: 'must' | 'must_not' | 'filter' = 'must',
    ) => {
      if (!frontpageImageMediaTypeId) return;

      const elastic = generateQuery(filter, query, condition);

      // The front-page-images exclusion is owned entirely by this hook. It must be reconciled on
      // every pass: added when front page images are not specifically requested, and removed when
      // they are. generateMustQuery preserves any prior 'must_not', so without stripping it here a
      // previously-baked-in exclusion is carried forward forever and selecting "Front Page Images"
      // would never un-hide them.
      const { must_not: priorMustNot, ...restBool } = elastic.query?.bool ?? {};
      const existingMustNot: any[] = Array.isArray(priorMustNot)
        ? priorMustNot
        : priorMustNot
        ? [priorMustNot]
        : [];

      // Drop only the front-page-media-type exclusion this hook may have added before; leave any
      // other must_not clauses untouched.
      const isFrontpageExclusion = (clause: any) => {
        const ids = clause?.terms?.mediaTypeId;
        return Array.isArray(ids) && ids.length === 1 && ids[0] === frontpageImageMediaTypeId;
      };
      let mustNot = existingMustNot.filter((c) => !isFrontpageExclusion(c));

      if (!filter.mediaTypeIds?.includes(frontpageImageMediaTypeId)) {
        // Do not include front page images in results unless they are specifically requested.
        const frontpageMustNot = generateMustNotQuery({
          mediaTypeIds: [frontpageImageMediaTypeId],
        }).query?.bool?.must_not;
        if (Array.isArray(frontpageMustNot)) mustNot = [...mustNot, ...frontpageMustNot];
        else if (frontpageMustNot) mustNot = [...mustNot, frontpageMustNot];
      }

      return {
        ...elastic,
        query: {
          ...elastic.query,
          bool: {
            ...restBool,
            ...(mustNot.length ? { must_not: mustNot } : {}),
          },
        },
      };
    },
    [frontpageImageMediaTypeId],
  );
};
