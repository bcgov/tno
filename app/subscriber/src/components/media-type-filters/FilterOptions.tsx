import React, { useEffect, useState } from 'react';
import {
  FaCalendar,
  FaCanadianMapleLeaf,
  FaGlobe,
  FaNewspaper,
  FaTowerCell,
} from 'react-icons/fa6';
import { useContent, useLookup } from 'store/hooks';
import { useUsers } from 'store/hooks';
import { useAppStore } from 'store/slices';
import {
  Button,
  ButtonHeight,
  ContentTypeName,
  IFilterSettingsModel,
  ISubscriberUserModel,
  useWindowSize,
} from 'tno-core';

import { defaultFilter, FilterOptionTypes } from './constants';
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
    | 'eventOfTheDay'
    | 'pressGalleryFilter'
    | 'searchResults';
}

/**
 * Component to filter media types on content list pages
 * @param filterStoreName - name of the filter in redux context
 */
export const FilterOptions: React.FC<IMediaTypeFiltersProps> = ({ filterStoreName }) => {
  const [{ userInfo }, store] = useAppStore();
  const filterStoreMethod = determineStore(filterStoreName);
  const api = useUsers();
  const [active, setActive] = useState<FilterOptionTypes>(FilterOptionTypes.All);
  const savePreferences = async (filterPreference: FilterOptionTypes) => {
    if (userInfo) {
      try {
        const user = {
          ...userInfo,
          preferences: { ...userInfo.preferences, filterPreference },
        } as ISubscriberUserModel;
        await api.updateUser(user);
        store.storeUserInfo({ ...userInfo, preferences: user.preferences });
      } catch {}
    }
  };

  const [
    {
      [filterStoreName]: { filter },
    },
    { [filterStoreMethod]: storeFilter },
  ] = useContent();
  const { width } = useWindowSize();
  const [{ sources, mediaTypes }] = useLookup();

  const handleFilterClick = React.useCallback(
    (type: FilterOptionTypes) => {
      const updatedFilter: Partial<IFilterSettingsModel> = { ...defaultFilter };

      switch (type) {
        case FilterOptionTypes.Papers:
          updatedFilter.contentTypes = [ContentTypeName.PrintContent];
          break;
        case FilterOptionTypes.RadioTV:
          updatedFilter.contentTypes = [ContentTypeName.AudioVideo];
          updatedFilter.mediaTypeIds = mediaTypes
            .filter((p) => p.name !== 'Events')
            .map((p) => p.id);
          break;
        case FilterOptionTypes.Internet:
          updatedFilter.contentTypes = [ContentTypeName.Internet];
          updatedFilter.mediaTypeIds = mediaTypes
            .filter((p) => p.name !== 'Events' && p.name !== 'CP Wire')
            .map((p) => p.id);
          break;
        case FilterOptionTypes.CPNews:
          updatedFilter.contentTypes = [ContentTypeName.Internet];
          updatedFilter.sourceIds = [sources.find((s) => s.code === 'CPNEWS')?.id ?? 0];
          break;
        case FilterOptionTypes.Events:
          updatedFilter.mediaTypeIds = [mediaTypes.find((s) => s.name === 'Events')?.id ?? 0];
          break;
        case FilterOptionTypes.All:
        default:
          break;
      }
      storeFilter({
        ...filter,
        ...updatedFilter,
      });
    },
    [filter, mediaTypes, sources, storeFilter],
  );

  const getClassName = (type: FilterOptionTypes) => (type === active ? 'active' : 'inactive');

  useEffect(() => {
    // Initial Check: Ensure initial preferences have been processed before proceeding
    if (!filter.contentTypes?.length && !filter.mediaTypeIds?.length && !!userInfo?.preferences) {
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
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, mediaTypes]);

  const filters = [
    { type: FilterOptionTypes.Papers, label: 'PAPERS', icon: <FaNewspaper /> },
    { type: FilterOptionTypes.RadioTV, label: 'RADIO/TV', icon: <FaTowerCell /> },
    { type: FilterOptionTypes.Internet, label: 'ONLINE', icon: <FaGlobe /> },
    { type: FilterOptionTypes.CPNews, label: 'CP NEWS', icon: <FaCanadianMapleLeaf /> },
    { type: FilterOptionTypes.Events, label: 'EVENTS', icon: <FaCalendar /> },
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
          onClick={() => {
            handleFilterClick(filter.type);
            savePreferences(filter.type);
          }}
        >
          {width && width < 768 ? filter.icon ?? 'All' : filter.label}
        </Button>
      ))}
    </styled.HomeFilters>
  );
};
