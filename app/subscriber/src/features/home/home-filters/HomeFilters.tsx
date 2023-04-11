import React from 'react';
import {
  Button,
  ButtonHeight,
  ButtonVariant,
  ContentTypeName,
  IContentFilter,
  IContentModel,
  Page,
} from 'tno-core';

import { HomeFilterType } from '../constants';
import * as styled from './styled';

export interface IHomeFilterProps {
  fetch: (filter: IContentFilter) => Promise<Page<IContentModel>>;
}

/**
 * Component for displaying the home filters
 * @param fetch performs tha api call to gather the appropriate content
 * @returns Home filter component
 */
export const HomeFilters: React.FC<IHomeFilterProps> = ({ fetch }) => {
  const [active, setActive] = React.useState<HomeFilterType>(HomeFilterType.Papers);
  const handleFilterClick = (type: HomeFilterType) => {
    setActive(type);
  };
  const getClassName = (type: HomeFilterType) => {
    return type === active ? 'active' : 'inactive';
  };

  React.useEffect(() => {
    switch (active) {
      case HomeFilterType.Papers:
        fetch({ contentTypes: [ContentTypeName.PrintContent] });
        break;
      case HomeFilterType.RadioTV:
        fetch({ contentTypes: [ContentTypeName.Snippet] });
        break;
      case HomeFilterType.Internet:
        fetch({ contentTypes: [ContentTypeName.Story] });
        break;
      case HomeFilterType.CPNews:
        fetch({ contentTypes: [ContentTypeName.Story], sourceIds: [174] });
        break;
      default:
        fetch({ contentTypes: [ContentTypeName.PrintContent] });
    }
  }, [active, fetch]);

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
