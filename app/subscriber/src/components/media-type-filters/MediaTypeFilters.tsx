import React, { useEffect, useState } from 'react';
import { useContent, useLookup } from 'store/hooks';
import { Button, ButtonHeight, ContentTypeName, IFilterSettingsModel } from 'tno-core';

import { MediaFilterTypes } from './constants';
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
export const MediaTypeFilters: React.FC<IMediaTypeFiltersProps> = ({ filterStoreName }) => {
  const [active, setActive] = useState<MediaFilterTypes>(MediaFilterTypes.Papers);
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

  const handleFilterClick = (type: MediaFilterTypes) => {
    const updatedFilter = { ...defaultFilter };

    switch (type) {
      case MediaFilterTypes.Papers:
        updatedFilter.contentTypes = [ContentTypeName.PrintContent];
        break;
      case MediaFilterTypes.RadioTV:
        updatedFilter.contentTypes = [ContentTypeName.AudioVideo];
        updatedFilter.mediaTypeIds = mediaTypes.filter((p) => p.name !== 'Events').map((p) => p.id);
        break;
      case MediaFilterTypes.Internet:
        updatedFilter.contentTypes = [ContentTypeName.Internet];
        updatedFilter.sourceIds = sources.filter((s) => s.code !== 'CPNEWS').map((s) => s.id);
        updatedFilter.mediaTypeIds = mediaTypes.filter((p) => p.name !== 'Events').map((p) => p.id);
        break;
      case MediaFilterTypes.CPNews:
        updatedFilter.contentTypes = [ContentTypeName.Internet];
        updatedFilter.sourceIds = [sources.find((s) => s.code === 'CPNEWS')?.id ?? 0];
        break;
      case MediaFilterTypes.Events:
        updatedFilter.mediaTypeIds = [mediaTypes.find((s) => s.name === 'Events')?.id ?? 0];
        break;
      case MediaFilterTypes.All:
        break;
      default:
        break;
    }
    storeFilter({ ...filter, ...updatedFilter });
  };

  const getClassName = (type: MediaFilterTypes) => (type === active ? 'active' : 'inactive');

  useEffect(() => {
    if (!filter.contentTypes?.length && !filter.mediaTypeIds?.length) {
      setActive(MediaFilterTypes.All);
    } else if (
      filter.mediaTypeIds?.includes(mediaTypes.find((s) => s.name === 'Events')?.id ?? 0)
    ) {
      setActive(MediaFilterTypes.Events);
    } else {
      // currently only support one content type at a time (with the exception of the all filter)
      if (!!filter?.contentTypes?.length) {
        switch (filter.contentTypes[0]) {
          case ContentTypeName.PrintContent:
            setActive(MediaFilterTypes.Papers);
            break;
          case ContentTypeName.AudioVideo:
            setActive(MediaFilterTypes.RadioTV);
            break;
          case ContentTypeName.Internet:
            if (filter.sourceIds?.length === 1) {
              setActive(MediaFilterTypes.CPNews);
              break;
            } else {
              setActive(MediaFilterTypes.Internet);
              break;
            }
          default:
            setActive(MediaFilterTypes.All);
        }
      }
    }
  }, [filter, mediaTypes]);

  const filters = [
    { type: MediaFilterTypes.Papers, label: 'PAPERS' },
    { type: MediaFilterTypes.RadioTV, label: 'RADIO/TV' },
    { type: MediaFilterTypes.Internet, label: 'INTERNET' },
    { type: MediaFilterTypes.CPNews, label: 'CP NEWS' },
    { type: MediaFilterTypes.Events, label: 'EVENTS' },
    { type: MediaFilterTypes.All, label: 'ALL' },
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
