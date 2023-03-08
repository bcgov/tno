import { ToolBar } from 'components/tool-bar/ToolBar';
import { ContentTypeName, useLookupOptions } from 'hooks';
import { noop } from 'lodash';
import React from 'react';
import { useContent } from 'store/hooks';
import { filterEnabled } from 'store/hooks/lookup/utils';
import { storeFilterAdvanced } from 'store/slices';
import {
  FieldSize,
  instanceOfIOption,
  IOptionItem,
  OptionItem,
  replaceQueryParams,
  Select,
} from 'tno-core';

import { fieldTypes, timeFrames } from '../list-view/constants';
import { IContentListAdvancedFilter } from '../list-view/interfaces';
import { queryToFilter, queryToFilterAdvanced } from '../list-view/utils';
import { AdvancedSearchSection, CreateNewSection, InputOption } from '../tool-bar/sections/filter';
import { AdvancedMorningReport } from './AdvancedMorningReport';
import { FilteredContentSection, TitleSection } from './filter-sections';
import { IMorningReportFilter } from './interfaces';
import * as styled from './styled';

export interface IMorningReportFilterProps {
  onSearch: (filter: IMorningReportFilter) => void;
}

export const MorningReportFilter: React.FC<IMorningReportFilterProps> = ({ onSearch }) => {
  const [
    { morningReportFilter: filter, filterAdvanced },
    { storeMorningReportFilter, findContent },
  ] = useContent();
  const [{ productOptions: pOptions, sourceOptions }] = useLookupOptions();

  const [productOptions, setProductOptions] = React.useState<IOptionItem[]>([]);

  const timeFrame = timeFrames[Number(filter.timeFrame)];

  // pass filter and filter advanced so we don't lose the advanced filter when we change the filter
  const onAdvancedFilterChange = (advancedFilter: IContentListAdvancedFilter) => {
    alert('here');
    storeFilterAdvanced(advancedFilter);
  };

  React.useEffect(() => {
    // Extract query string values and place them into redux store.
    storeMorningReportFilter(
      queryToFilter(
        { ...filter, contentType: ContentTypeName.PrintContent },
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
