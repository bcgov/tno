import React from 'react';
import { useContent } from 'store/hooks';
import { Checkbox, Row } from 'tno-core';

export const SearchInGroup: React.FC = () => {
  const [
    {
      search: { filter },
    },
    { storeSearchFilter: storeFilter },
  ] = useContent();

  return (
    <Row className="options expanded space-top">
      <label className="search-in-label">Search in: </label>
      <Checkbox
        id="chkInHeadline"
        label="Headline"
        checked={filter.inHeadline}
        onChange={(e) => {
          storeFilter({ ...filter, inHeadline: e.target.checked });
        }}
      />
      <Checkbox
        id="chkInByline"
        label="Byline"
        checked={filter.inByline}
        onChange={(e) => storeFilter({ ...filter, inByline: e.target.checked })}
      />
      <Checkbox
        id="chkInStory"
        label="Story text"
        checked={filter.inStory}
        onChange={(e) => storeFilter({ ...filter, inStory: e.target.checked })}
      />
      <Checkbox
        id="chkInProgram"
        label="Program/Show"
        checked={filter.inProgram}
        onChange={(e) => storeFilter({ ...filter, inProgram: e.target.checked })}
      />
    </Row>
  );
};
