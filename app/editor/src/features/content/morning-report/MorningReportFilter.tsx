import { ToolBar } from 'components/tool-bar/ToolBar';
import { ContentTypeName, useLookupOptions } from 'hooks';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
import { storeFilterAdvanced } from 'store/slices';
import { IOptionItem, OptionItem, replaceQueryParams } from 'tno-core';

import { fieldTypes } from '../list-view/constants';
import { queryToFilter, queryToFilterAdvanced } from '../list-view/utils';
import { CreateNewSection } from '../tool-bar/sections/filter';
import { AdvancedMorningReport } from './AdvancedMorningReport';
import { defaultReportFilter } from './constants/defaultReportFilter';
import { FilteredContentSection, TitleSection } from './filter-sections';
import { IMorningReportFilter } from './interfaces';
import * as styled from './styled';

export interface IMorningReportFilterProps {
  onSearch: (filter: IMorningReportFilter) => void;
}

export const MorningReportFilter: React.FC<IMorningReportFilterProps> = ({ onSearch }) => {
  const [{ filterMorningReport: filter, filterAdvanced }, { storeMorningReportFilter }] =
    useContent();
  const [{ productOptions: pOptions }] = useLookupOptions();
  const [{ sources }] = useLookup();

  const [, setProductOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    // Extract query string values and place them into redux store.
    if (!window.location.search) {
      replaceQueryParams(defaultReportFilter(sources), { includeEmpty: false });
    }
    storeMorningReportFilter(
      queryToFilter(
        {
          ...defaultReportFilter(sources),
        },
        window.location.search,
      ),
    );
    storeFilterAdvanced(
      queryToFilterAdvanced(
        { ...filterAdvanced, fieldType: fieldTypes[0].value },
        window.location.search,
      ),
    );
    // Only want this to run on the first load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    setProductOptions([new OptionItem<number>('Any', 0), ...pOptions]);
  }, [pOptions]);

  const onFilterChange = (filter: IMorningReportFilter) => {
    storeMorningReportFilter(filter);
    replaceQueryParams(filter, { includeEmpty: false });
  };

  return (
    <styled.MorningReportFilter>
      <ToolBar>
        <TitleSection />
        <CreateNewSection />
        <FilteredContentSection
          onSearch={onSearch}
          filter={filter}
          onFilterChange={onFilterChange}
        />
        <AdvancedMorningReport onSearch={onSearch} />
      </ToolBar>
    </styled.MorningReportFilter>
  );
};
