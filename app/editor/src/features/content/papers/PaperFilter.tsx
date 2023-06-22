import React from 'react';
import { useContent, useLookup, useLookupOptions } from 'store/hooks';
import { storeFilterAdvanced } from 'store/slices';
import { ContentTypeName, IOptionItem, OptionItem, replaceQueryParams, ToolBar } from 'tno-core';

import { CreateNewSection } from '../list-view/components/tool-bar/filter';
import { advancedSearchKeys } from '../list-view/constants';
import { queryToFilter, queryToFilterAdvanced } from '../list-view/utils';
import { AdvancedFilter, ContentFilter } from './components';
import { defaultPaperFilter } from './constants';
import { IPaperFilter } from './interfaces';
import * as styled from './styled';

export interface IPaperFilterProps {
  /** Event fired when search is executed. */
  onSearch: (filter: IPaperFilter) => void;
}

/**
 * Provides a filter for the papers.
 * @param param0 Component properties.
 * @returns Component.
 */
export const PaperFilter: React.FC<IPaperFilterProps> = ({ onSearch }) => {
  const [{ filterPaper: filter, filterAdvanced }, { storeFilterPaper }] = useContent();
  const [{ productOptions: pOptions }] = useLookupOptions();
  const [{ sources }] = useLookup();

  const [, setProductOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    // Extract query string values and place them into redux store.
    if (!window.location.search) {
      replaceQueryParams(defaultPaperFilter(sources), { includeEmpty: false });
    }
    storeFilterPaper(
      queryToFilter(
        {
          ...defaultPaperFilter(sources),
        },
        window.location.search,
      ),
    );
    storeFilterAdvanced(
      queryToFilterAdvanced(
        { ...filterAdvanced, fieldType: advancedSearchKeys.Headline },
        window.location.search,
      ),
    );
    // Only want this to run on the first load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    setProductOptions([new OptionItem<number>('Any', 0), ...pOptions]);
  }, [pOptions]);

  const onFilterChange = (filter: IPaperFilter) => {
    const newFilter = { ...filter, pageIndex: 0 };
    storeFilterPaper(newFilter);
    replaceQueryParams(newFilter, { includeEmpty: false });
  };

  return (
    <styled.PaperFilter>
      <ToolBar>
        <CreateNewSection contentTypes={[ContentTypeName.PrintContent, ContentTypeName.Image]} />
        <ContentFilter filter={filter} onFilterChange={onFilterChange} />
        <AdvancedFilter onSearch={onSearch} filter={filter} onFilterChange={onFilterChange} />
      </ToolBar>
    </styled.PaperFilter>
  );
};
