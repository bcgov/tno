import {
  ISubMediaGroupExpanded,
  ISubMediaGroupItem,
} from 'features/search-page/components/advanced-search/interfaces';
import { IMediaTypeModel, ISourceModel } from 'tno-core';

/**
 * Create Sub Media Groups
 * @param sources
 * @param mediaTypes
 * @param setSubMediaGroups
 * @param setMediaGroupExpanded - optional
 */
export const createSubMediaGroups = (
  sources: ISourceModel[],
  mediaTypes: IMediaTypeModel[],
  setSubMediaGroups: (groups: ISubMediaGroupItem[]) => void,
  setMediaGroupExpanded?: (expanded: ISubMediaGroupExpanded) => void,
) => {
  // exit early if inputs are not set completely
  if (sources.length === 0 || mediaTypes.length === 0) return;

  let mediaTypeSourceLookup: { [name: string]: ISourceModel[] } = {};
  const allSourcesKey: string = 'All';
  mediaTypeSourceLookup[allSourcesKey] = [];
  // prime the dictionary - already in sort order set on Media Type
  mediaTypes.forEach((mt) => {
    mediaTypeSourceLookup[mt.name] = [];
  });
  sources.forEach((source) => {
    mediaTypeSourceLookup[allSourcesKey].push(source);
    source.mediaTypeSearchMappings.forEach((mapping) => {
      mediaTypeSourceLookup[mapping.name].push(source);
    });
  });
  // Remove Media Type entries with no assigned Sources
  // Could also exclude specific Media Types her if required
  mediaTypeSourceLookup = Object.fromEntries(
    Object.entries(mediaTypeSourceLookup).filter(([_, v]) => v.length > 0),
  );

  let subMediaGroups: ISubMediaGroupItem[] = [];
  let mediaGroupExpandedStates: ISubMediaGroupExpanded = {};
  for (let key in mediaTypeSourceLookup) {
    // Use `key` and `value`
    let value = mediaTypeSourceLookup[key];
    subMediaGroups.push({
      key: key,
      label: key,
      options: value,
    });
    // initialize state model for what groups are expanded/collapsed
    mediaGroupExpandedStates[key] = false;
  }

  setSubMediaGroups(subMediaGroups);
  if (setMediaGroupExpanded) {
    setMediaGroupExpanded(mediaGroupExpandedStates);
  }
};
