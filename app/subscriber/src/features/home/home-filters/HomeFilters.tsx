import { IContentListFilter } from 'features/content/list-view/interfaces';
import React, { useEffect, useState } from 'react';
import { useContent, useLookup } from 'store/hooks';
import { Button, ButtonHeight, ContentTypeName } from 'tno-core';

import { HomeFilterType } from '../constants';
import * as styled from './styled';

export interface IHomeFilterProps {}

/**
 * Component that renders the toggle buttons for filtering content on the Home page.
 * @returns Home filter toggles.
 */
export const HomeFilters: React.FC<IHomeFilterProps> = () => {
  const [active, setActive] = useState<HomeFilterType>(HomeFilterType.Papers);
  const [{ filter }, { storeFilter }] = useContent();
  const [{ sources }] = useLookup();

  const defaultFilter: Partial<IContentListFilter> = {
    contentTypes: [],
    sourceIds: [],
    excludeSourceIds: [],
  };

  const handleFilterClick = (type: HomeFilterType) => {
    const updatedFilter = { ...defaultFilter };

    switch (type) {
      case HomeFilterType.Papers:
        updatedFilter.contentTypes = [ContentTypeName.PrintContent];
        break;
      case HomeFilterType.RadioTV:
        updatedFilter.contentTypes = [ContentTypeName.AudioVideo];
        break;
      case HomeFilterType.Internet:
        updatedFilter.contentTypes = [ContentTypeName.Story];
        updatedFilter.excludeSourceIds = [sources.find((s) => s.code === 'CPNEWS')?.id ?? 0];
        break;
      case HomeFilterType.CPNews:
        updatedFilter.contentTypes = [ContentTypeName.Story];
        updatedFilter.sourceIds = [sources.find((s) => s.code === 'CPNEWS')?.id ?? 0];
        break;
      case HomeFilterType.All:
        break;
      default:
        break;
    }

    storeFilter({ ...filter, ...updatedFilter });
  };

  const getClassName = (type: HomeFilterType) => (type === active ? 'active' : 'inactive');

  useEffect(() => {
    if (!filter.contentTypes?.length) {
      setActive(HomeFilterType.All);
    } else {
      // currently only support one content type at a time (with the exception of the all filter)
      switch (filter.contentTypes[0]) {
        case ContentTypeName.PrintContent:
          setActive(HomeFilterType.Papers);
          break;
        case ContentTypeName.AudioVideo:
          setActive(HomeFilterType.RadioTV);
          break;
        case ContentTypeName.Story:
          setActive(!!filter.sourceIds?.length ? HomeFilterType.CPNews : HomeFilterType.Internet);
          break;
        default:
          setActive(HomeFilterType.All);
      }
    }
  }, [filter]);

  const filters = [
    { type: HomeFilterType.All, label: 'ALL' },
    { type: HomeFilterType.Papers, label: 'PAPERS' },
    { type: HomeFilterType.RadioTV, label: 'RADIO/TV' },
    { type: HomeFilterType.Internet, label: 'INTERNET' },
    { type: HomeFilterType.CPNews, label: 'CP NEWS' },
  ];

  return (
    <styled.HomeFilters>
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
