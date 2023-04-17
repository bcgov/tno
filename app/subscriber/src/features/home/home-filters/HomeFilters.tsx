import React from 'react';
import { useContent } from 'store/hooks';
import { useSources } from 'store/hooks/admin';
import { Button, ButtonHeight, ButtonVariant, ContentTypeName } from 'tno-core';

import { HomeFilterType } from '../constants';
import * as styled from './styled';

export interface IHomeFilterProps {}

/**
 * Component for displaying the home filters
 * @param fetch performs the api call to gather the appropriate content
 * @returns Home filter component
 */
export const HomeFilters: React.FC<IHomeFilterProps> = () => {
  const [active, setActive] = React.useState<HomeFilterType>(HomeFilterType.Papers);
  const [{ filter }, { storeFilter }] = useContent();
  const [{ sources }] = useSources();

  const handleFilterClick = (type: HomeFilterType) => {
    setActive(type);
  };
  const getClassName = (type: HomeFilterType) => {
    return type === active ? 'active' : 'inactive';
  };

  React.useEffect(() => {
    switch (active) {
      case HomeFilterType.Papers:
        storeFilter({ ...filter, contentTypes: [ContentTypeName.PrintContent], sourceIds: [] });
        break;
      case HomeFilterType.RadioTV:
        storeFilter({ ...filter, contentTypes: [ContentTypeName.Snippet], sourceIds: [] });
        break;
      case HomeFilterType.Internet:
        storeFilter({ ...filter, contentTypes: [ContentTypeName.Story], sourceIds: [] });
        break;
      case HomeFilterType.CPNews:
        storeFilter({
          ...filter,
          contentTypes: [ContentTypeName.Story],
          sourceIds: [sources.find((s) => s.name === 'CP News')?.id ?? 0],
        });
        break;
      default:
        storeFilter({ ...filter, contentTypes: [ContentTypeName.PrintContent] });
    }
    // only want the above to trigger when active changes not when the filter changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [active, sources]);

  return (
    <styled.HomeFilters>
      <Button
        rounded
        height={ButtonHeight.Small}
        className={getClassName(HomeFilterType.Papers)}
        onClick={() => handleFilterClick(HomeFilterType.Papers)}
      >
        PAPERS
      </Button>
      <Button
        variant={ButtonVariant.red}
        height={ButtonHeight.Small}
        rounded
        className={getClassName(HomeFilterType.RadioTV)}
        onClick={() => handleFilterClick(HomeFilterType.RadioTV)}
      >
        RADIO/TV
      </Button>
      <Button
        rounded
        height={ButtonHeight.Small}
        className={getClassName(HomeFilterType.Internet)}
        onClick={() => handleFilterClick(HomeFilterType.Internet)}
      >
        INTERNET
      </Button>
      <Button
        rounded
        height={ButtonHeight.Small}
        className={getClassName(HomeFilterType.CPNews)}
        onClick={() => handleFilterClick(HomeFilterType.CPNews)}
      >
        CP NEWS
      </Button>
    </styled.HomeFilters>
  );
};
