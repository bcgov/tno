import { TitleSection } from 'components/tool-bar';
import React from 'react';
import { FaEye, FaSun } from 'react-icons/fa';
import { useContent, useLookup, useLookupOptions } from 'store/hooks';
import { storeFilterAdvanced } from 'store/slices';
import { ContentTypeName, IOptionItem, OptionItem, replaceQueryParams, ToolBar } from 'tno-core';

import { advancedSearchKeys } from '../list-view/constants';
import { queryToFilter, queryToFilterAdvanced } from '../list-view/utils';
import { CreateNewSection } from '../tool-bar/sections/filter';
import { AdvancedFilter, ContentFilter } from './components';
import { defaultMorningReportsFilter } from './constants';
import { IMorningReportsFilter } from './interfaces';
import * as styled from './styled';

export interface IMorningReportsFilterProps {
  /** Event fired when search is executed. */
  onSearch: (filter: IMorningReportsFilter) => void;
}

/**
 * Provides a filter for the morning report.
 * @param param0 Component properties.
 * @returns Component.
 */
export const MorningReportsFilter: React.FC<IMorningReportsFilterProps> = ({ onSearch }) => {
  const [{ filterMorningReports: filter, filterAdvanced }, { storeFilterMorningReport }] =
    useContent();
  const [{ productOptions: pOptions }] = useLookupOptions();
  const [{ sources }] = useLookup();

  const [, setProductOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    // Extract query string values and place them into redux store.
    if (!window.location.search) {
      replaceQueryParams(defaultMorningReportsFilter(sources), { includeEmpty: false });
    }
    storeFilterMorningReport(
      queryToFilter(
        {
          ...defaultMorningReportsFilter(sources),
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

  const onFilterChange = (filter: IMorningReportsFilter) => {
    const newFilter = { ...filter, pageIndex: 0 };
    storeFilterMorningReport(newFilter);
    replaceQueryParams(newFilter, { includeEmpty: false });
  };

  return (
    <styled.MorningReportsFilter>
      <ToolBar>
        <TitleSection title="Morning Papers" picture={<FaSun />} label="VIEWING" icon={<FaEye />} />
        <CreateNewSection contentTypes={[ContentTypeName.PrintContent, ContentTypeName.Image]} />
        <ContentFilter filter={filter} onFilterChange={onFilterChange} />
        <AdvancedFilter onSearch={onSearch} filter={filter} onFilterChange={onFilterChange} />
      </ToolBar>
    </styled.MorningReportsFilter>
  );
};
