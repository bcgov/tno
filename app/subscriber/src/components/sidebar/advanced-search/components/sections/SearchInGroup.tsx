import React from 'react';
import { useContent } from 'store/hooks';
import { Checkbox, Row } from 'tno-core';

export const SearchInGroup: React.FC = () => {
  const [{ searchFilter: filter }, { storeSearchFilter: storeFilter }] = useContent();

  return (
    <Row className="options expanded space-top">
      <label className="search-in-label">Search in: </label>
      <Checkbox
        checked={filter.inHeadline}
        onChange={(e) => {
          storeFilter({ ...filter, inHeadline: e.target.checked });
        }}
      />
      <label>Headline</label>
      <Checkbox
        checked={filter.inByline}
        onChange={(e) => storeFilter({ ...filter, inByline: e.target.checked })}
      />
      <label>Byline</label>
      <Checkbox
        checked={filter.inStory}
        onChange={(e) => storeFilter({ ...filter, inStory: e.target.checked })}
      />
      <label>Story text</label>
    </Row>
  );
};
