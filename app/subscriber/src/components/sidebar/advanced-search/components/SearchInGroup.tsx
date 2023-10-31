import React from 'react';
import { Checkbox, Row, Show } from 'tno-core';

import { IAdvancedSearchFilter } from '../interfaces';

export interface ISearchInGroupProps {
  /** variable that keeps track of whether the sub-menu is expanded or not */
  searchExpanded: boolean;
  /** function that will update the search in terms */
  setAdvancedSearch: (advancedSearch: IAdvancedSearchFilter) => void;
  /** advanced search object, may start as undefined if nothing is set */
  advancedSearch: IAdvancedSearchFilter;
}

export const SearchInGroup: React.FC<ISearchInGroupProps> = ({
  searchExpanded,
  advancedSearch,
  setAdvancedSearch,
}) => {
  const [searchInOptions, setSearchInOptions] = React.useState({
    byline: advancedSearch.inByline,
    headline: advancedSearch.inHeadline,
    storyText: advancedSearch.inStory,
  });
  React.useEffect(() => {
    setAdvancedSearch({
      ...advancedSearch,
      searchTerm: advancedSearch.searchTerm,
      inHeadline: searchInOptions.headline ?? false,
      inByline: searchInOptions.byline ?? false,
      inStory: searchInOptions.storyText ?? false,
    });
    // only want to run when the options change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchInOptions]);

  return (
    <Show visible={searchExpanded}>
      <Row className="options expanded space-top">
        <label>Headline</label>
        <Checkbox
          checked={advancedSearch.inHeadline}
          onChange={(e) => {
            setSearchInOptions({ ...searchInOptions, headline: e.target.checked });
          }}
        />
        <label>Byline</label>
        <Checkbox
          checked={advancedSearch.inByline}
          onChange={(e) => setSearchInOptions({ ...searchInOptions, byline: e.target.checked })}
        />
        <label>Story text</label>
        <Checkbox
          checked={advancedSearch.inStory}
          onChange={(e) => setSearchInOptions({ ...searchInOptions, storyText: e.target.checked })}
        />
      </Row>
    </Show>
  );
};
