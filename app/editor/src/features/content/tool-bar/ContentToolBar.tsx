import { ToolBar } from 'components/tool-bar/ToolBar';
import React from 'react';
import { useContent } from 'store/hooks';
import { fromQueryString } from 'tno-core';

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
    let params = new URLSearchParams(search);
    const productIds = params.getAll('productIds');
    const productIdsIntArray = productIds.map((item) => parseInt(item));
    storeFilter({ ...filter, productIds: productIdsIntArray });
  };

  React.useEffect(() => {
    if (!!window.location.search) {
      if (!!search.productIds) extractProductIds(search.productIds);
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
