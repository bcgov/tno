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
    byline: false,
    headline: false,
    storyText: false,
  });
  React.useEffect(() => {
    setAdvancedSearch({
      ...advancedSearch,
      searchInField: {
        headline: searchInOptions.headline,
        byline: searchInOptions.byline,
        storyText: searchInOptions.storyText,
      },
    });
    // only want to run when the options change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchInOptions]);

  return (
    <Show visible={searchExpanded}>
      <Row justifyContent="space-evenly" className="options expanded space-top">
        <label>Headline</label>
        <Checkbox
          onChange={(e) => {
            setSearchInOptions({ ...searchInOptions, headline: e.target.checked });
          }}
        />
        <label>Byline</label>
        <Checkbox
          onChange={(e) =>
            e.target.checked
              ? setSearchInOptions({ ...searchInOptions, byline: true })
              : setSearchInOptions({ ...searchInOptions, byline: false })
          }
        />
        <label>Story text</label>
        <Checkbox
          onChange={(e) =>
            e.target.checked
              ? setSearchInOptions({ ...searchInOptions, storyText: true })
              : setSearchInOptions({ ...searchInOptions, storyText: false })
          }
        />
      </Row>
    </Show>
  );
};
