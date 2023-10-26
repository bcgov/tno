import { IContentListAdvancedFilter, IContentListFilter } from 'features/content/interfaces';
import React from 'react';
import { useContent } from 'store/hooks';
import { fromQueryString, ToolBar } from 'tno-core';

import {
  AdvancedSearchSection,
  CreateNewSection,
  FilterContentSection,
  ShowOnlySection,
} from './filter';

export interface IContentToolBarProps {
  onSearch: (filter: IContentListFilter & IContentListAdvancedFilter) => void;
}

/**
 * Uses the ToolBar component to create a toolbar specific to the content filter
 * @param onSearch Inform the parent of the request to search.
 * @returns A content filter toolbar
 */
export const ContentToolBar: React.FC<IContentToolBarProps> = ({ onSearch }) => {
  const [{ filterAdvanced }, { storeFilterAdvanced }] = useContent();
  const search = fromQueryString(window.location.search);

  React.useEffect(() => {
    if (!!window.location.search) {
      Object.keys(search).forEach(function (key) {
        if (key in filterAdvanced && search[key] !== undefined) {
          storeFilterAdvanced({ ...filterAdvanced, [key]: search[key] });
        }
      });
    }
    // parse productIds for each one and put into array
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <ToolBar>
      {/* first section */}
      <CreateNewSection />
      {/* second section */}
      <FilterContentSection />
      {/* third section */}
      <ShowOnlySection />
      {/* fourth section */}
      <AdvancedSearchSection />
    </ToolBar>
  );
};
