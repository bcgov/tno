import { instanceOfIOption } from 'components/form';
import { ToolBar } from 'components/tool-bar/ToolBar';
import { useContent } from 'store/hooks';
import { replaceQueryParams } from 'tno-core';

import { IContentListAdvancedFilter, IContentListFilter } from '../list-view/interfaces';
import {
  AdvancedSearchSection,
  CreateNewSection,
  FilterContentSection,
  ShowOnlySection,
} from './sections';

export interface IContentToolBarProps {
  onSearch: (filter: IContentListFilter & IContentListAdvancedFilter) => void;
}

/**
 * Uses the ToolBar component to create a toolbar specific to the content filter
 * @param onSearch Inform the parent of the request to search.
 * @returns A content filter toolbar
 */
export const ContentToolBar: React.FC<IContentToolBarProps> = ({ onSearch }) => {
  const [, { storeFilter, storeFilterAdvanced }] = useContent();

  const onFilterChange = (filter: IContentListFilter) => {
    storeFilter(filter);
    replaceQueryParams(filter, { includeEmpty: false });
  };

  const onAdvancedFilterChange = (filter: IContentListAdvancedFilter) => {
    storeFilterAdvanced(filter);
    replaceQueryParams(filter, {
      includeEmpty: false,
      convertObject: (value) => {
        if (instanceOfIOption(value)) return value.value;
        return value.toString();
      },
    });
  };
  return (
    <ToolBar>
      {/* first section */}
      <CreateNewSection />
      {/* second section */}
      <FilterContentSection onChange={onFilterChange} />
      {/* third section */}
      <ShowOnlySection onChange={onFilterChange} />
      {/* fourth section */}
      <AdvancedSearchSection onSearch={onSearch} onChange={onAdvancedFilterChange} />
    </ToolBar>
  );
};
