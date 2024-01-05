import React from 'react';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { useLookupStore } from 'store/slices';
import { generateMustNotQuery, generateQuery, IFilterSettingsModel, Settings } from 'tno-core';

/**
 * Hook provides helper function to generate elasticsearch query.
 * It also removes front page images from any query that isn't specifically asking for them.
 * @returns Function to generate an elasticsearch query from configuration.
 */
export const useElastic = () => {
  const [{ isReady, settings }] = useLookup();
  const [{ frontPageImagesMediaTypeId }, { storeSettingsFrontPageImagesMediaTypeId }] =
    useLookupStore();

  React.useEffect(() => {
    if (isReady) {
      const frontPageImagesMediaTypeId = settings.find(
        (s) => s.name === Settings.FrontPageImageMediaType,
      )?.value;
      if (frontPageImagesMediaTypeId)
        storeSettingsFrontPageImagesMediaTypeId(+frontPageImagesMediaTypeId);
      else toast.error(`Configuration settings '${Settings.FrontPageImageMediaType}' is required.`);
    }
  }, [isReady, settings, storeSettingsFrontPageImagesMediaTypeId]);

  return React.useCallback(
    (
      filter: IFilterSettingsModel,
      query?: any,
      condition: 'must' | 'must_not' | 'filter' = 'must',
    ) => {
      var elastic = generateQuery(filter, query, condition);
      // TODO: The first time this executes there will be no "frontPageImagesMediaTypeId" values...
      // This is because React is horrible and doesn't have a way to await a state value...
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
