import React, { useEffect, useState } from 'react';
import { useContent, useLookup } from 'store/hooks';
import { Button, ButtonHeight, ContentTypeName, IFilterSettingsModel } from 'tno-core';

import { FilterOptionTypes } from './constants';
import * as styled from './styled';
import { determineStore } from './utils';

export interface IMediaTypeFiltersProps {
  filterStoreName:
    | 'home'
    | 'todaysCommentary'
    | 'search'
    | 'frontPage'
    | 'mediaType'
    | 'myMinister'
    | 'topStories'
    | 'avOverview'
    | 'pressGalleryFilter';
}

/**
 * Component to filter media types on content list pages
 * @param filterStoreName - name of the filter in redux context
 */
export const FilterOption: React.FC<IMediaTypeFiltersProps> = ({ filterStoreName }) => {
  const [active, setActive] = useState<FilterOptionTypes>(FilterOptionTypes.Papers);
  const filterStoreMethod = determineStore(filterStoreName);
  const [
    {
      [filterStoreName]: { filter },
    },
    { [filterStoreMethod]: storeFilter },
  ] = useContent();
  const [{ sources, mediaTypes }] = useLookup();

  const defaultFilter: Partial<IFilterSettingsModel> = {
    contentTypes: [],
    sourceIds: [],
    mediaTypeIds: [],
  };

  const handleFilterClick = (type: FilterOptionTypes) => {
    const updatedFilter = { ...defaultFilter };

    switch (type) {
      case FilterOptionTypes.Papers:
        updatedFilter.contentTypes = [ContentTypeName.PrintContent];
        break;
      case FilterOptionTypes.RadioTV:
        updatedFilter.contentTypes = [ContentTypeName.AudioVideo];
        updatedFilter.mediaTypeIds = mediaTypes.filter((p) => p.name !== 'Events').map((p) => p.id);
        break;
      case FilterOptionTypes.Internet:
        updatedFilter.contentTypes = [ContentTypeName.Internet];
        updatedFilter.sourceIds = sources.filter((s) => s.code !== 'CPNEWS').map((s) => s.id);
        updatedFilter.mediaTypeIds = mediaTypes.filter((p) => p.name !== 'Events').map((p) => p.id);
        break;
      case FilterOptionTypes.CPNews:
        updatedFilter.contentTypes = [ContentTypeName.Internet];
        updatedFilter.sourceIds = [sources.find((s) => s.code === 'CPNEWS')?.id ?? 0];
        break;
      case FilterOptionTypes.Events:
        updatedFilter.mediaTypeIds = [mediaTypes.find((s) => s.name === 'Events')?.id ?? 0];
        break;
      case FilterOptionTypes.All:
        break;
      default:
        break;
    }
    storeFilter({ ...filter, ...updatedFilter });
  };

  const getClassName = (type: FilterOptionTypes) => (type === active ? 'active' : 'inactive');

  useEffect(() => {
    if (!filter.contentTypes?.length && !filter.mediaTypeIds?.length) {
      setActive(FilterOptionTypes.All);
    } else if (
      filter.mediaTypeIds?.includes(mediaTypes.find((s) => s.name === 'Events')?.id ?? 0)
    ) {
      setActive(FilterOptionTypes.Events);
    } else {
      // currently only support one content type at a time (with the exception of the all filter)
      if (!!filter?.contentTypes?.length) {
        switch (filter.contentTypes[0]) {
          case ContentTypeName.PrintContent:
            setActive(FilterOptionTypes.Papers);
            break;
          case ContentTypeName.AudioVideo:
            setActive(FilterOptionTypes.RadioTV);
            break;
          case ContentTypeName.Internet:
            if (filter.sourceIds?.length === 1) {
              setActive(FilterOptionTypes.CPNews);
              break;
            } else {
              setActive(FilterOptionTypes.Internet);
              break;
            }
          default:
            setActive(FilterOptionTypes.All);
        }
      }
    }
  }, [filter, mediaTypes]);

  const filters = [
    { type: FilterOptionTypes.Papers, label: 'PAPERS' },
    { type: FilterOptionTypes.RadioTV, label: 'RADIO/TV' },
    { type: FilterOptionTypes.Internet, label: 'INTERNET' },
    { type: FilterOptionTypes.CPNews, label: 'CP NEWS' },
    { type: FilterOptionTypes.Events, label: 'EVENTS' },
    { type: FilterOptionTypes.All, label: 'ALL' },
  ];

  return (
    <styled.HomeFilters className="filter-buttons">
      {filters.map((filter) => (
        <Button
          key={filter.type}
          rounded
          height={ButtonHeight.Small}
          className={getClassName(filter.type)}
          onClick={() => handleFilterClick(filter.type)}
        >
          {filter.label}
        </Button>
      ))}
    </styled.HomeFilters>
  );
};
