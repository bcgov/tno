import { ToolBar } from 'components/tool-bar/ToolBar';
import React from 'react';
import { useContent } from 'store/hooks';
import { fromQueryString, instanceOfIOption, replaceQueryParams } from 'tno-core';

import { IContentListAdvancedFilter, IContentListFilter } from '../list-view/interfaces';
import {
  AdvancedSearchSection,
  CreateNewSection,
  FilterContentSection,
  ShowOnlySection,
} from './sections/filter';

export interface IContentToolBarProps {
  onSearch: (filter: IContentListFilter & IContentListAdvancedFilter) => void;
}

/**
 * Uses the ToolBar component to create a toolbar specific to the content filter
 * @param onSearch Inform the parent of the request to search.
 * @returns A content filter toolbar
 */
export const ContentToolBar: React.FC<IContentToolBarProps> = ({ onSearch }) => {
  const [{ filter, filterAdvanced }, { storeFilter, storeFilterAdvanced }] = useContent();
  const search = fromQueryString(window.location.search);
  // extract every value from window.location.search that is productIds
  // and put into an array
  const extractProductIds = (search: string) => {
    const productIds = search.split('&').filter((item) => item.includes('productIds'));
    const productIdsArray = productIds.map((item) => Number(item.split('=')[1]));
    storeFilter({ ...filter, productIds: productIdsArray });
  };

  React.useEffect(() => {
    if (!!search.productIds) extractProductIds(window.location.search);
    Object.keys(search).forEach(function (key) {
      if (key in filterAdvanced && search[key] !== undefined) {
        storeFilterAdvanced({ ...filterAdvanced, [key]: search[key] });
      }
    });
    // parse productIds for each one and put into array
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const onFilterChange = (filter: IContentListFilter) => {
    storeFilter(filter);
    replaceQueryParams({ ...filter, ...filterAdvanced }, { includeEmpty: false });
  };
  // pass filter and filter advanced so we dont lose the advanced filter when we change the filter
  const onAdvancedFilterChange = (advancedFilter: IContentListAdvancedFilter) => {
    storeFilterAdvanced(advancedFilter);
    replaceQueryParams(
      { ...filter, ...advancedFilter },
      {
        includeEmpty: false,
        convertObject: (value) => {
          if (instanceOfIOption(value)) return value.value;
          return value.toString();
        },
      },
    );
  };

  return (
    <ToolBar>
      {/* first section */}
      <CreateNewSection />
      {/* second section */}
      <FilterContentSection
        onChange={onFilterChange}
        onAdvancedFilterChange={onAdvancedFilterChange}
        onSearch={onSearch}
      />
      {/* third section */}
      <ShowOnlySection onChange={onFilterChange} />
      {/* fourth section */}
      <AdvancedSearchSection onSearch={onSearch} onChange={onAdvancedFilterChange} />
    </ToolBar>
  );
};
