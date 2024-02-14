import {
  ISubMediaGroupExpanded,
  ISubMediaGroupItem,
} from 'features/search-page/components/advanced-search/interfaces';
import { useEffect, useMemo, useState } from 'react';
import { IMediaTypeModel, ISourceModel } from 'tno-core';

/**
 * Custom hook to create and manage sub media groups.
 * @param sources Array of source models.
 * @param mediaTypes Array of media type models.
 * @returns An object containing subMediaGroups, mediaGroupExpanded, and setMediaGroupExpanded function.
 */
export const useSubMediaGroups = (
  sources: ISourceModel[],
  mediaTypes: IMediaTypeModel[],
): {
  subMediaGroups: ISubMediaGroupItem[];
  mediaGroupExpanded: ISubMediaGroupExpanded;
  setMediaGroupExpanded: (groupExpanded: ISubMediaGroupExpanded) => void;
} => {
  const [subMediaGroups, setSubMediaGroups] = useState<ISubMediaGroupItem[]>([]);
  const [mediaGroupExpanded, setMediaGroupExpanded] = useState<ISubMediaGroupExpanded>({});

  // Determine mediaTypeSourceLookup only when sources or mediaTypes change
  const mediaTypeSourceLookup = useMemo(() => {
    const lookup: { [key: string]: ISourceModel[] } = { All: [...sources] }; // Initialize with 'All' key
    mediaTypes.forEach((mt) => {
      lookup[mt.name] = []; // Initialize media type keys
    });
    sources.forEach((source) => {
      source.mediaTypeSearchMappings.forEach((mapping) => {
        if (lookup[mapping.name]) {
          lookup[mapping.name].push(source);
        }
      });
    });
    // Filter out media types with no sources
    return Object.fromEntries(Object.entries(lookup).filter(([_, value]) => value.length > 0));
  }, [sources, mediaTypes]);

  useEffect(() => {
    const subGroups: ISubMediaGroupItem[] = Object.keys(mediaTypeSourceLookup).map((key) => ({
      key,
      label: key,
      options: mediaTypeSourceLookup[key],
    }));

    const expandedStates: ISubMediaGroupExpanded = subGroups.reduce(
      (acc: ISubMediaGroupExpanded, { key }) => {
        acc[key] = false; // Default all groups to not expanded
        return acc;
      },
      {},
    );

    setSubMediaGroups(subGroups);
    setMediaGroupExpanded(expandedStates);
  }, [mediaTypeSourceLookup]);

  return {
    subMediaGroups,
    mediaGroupExpanded,
    setMediaGroupExpanded,
  };
};
