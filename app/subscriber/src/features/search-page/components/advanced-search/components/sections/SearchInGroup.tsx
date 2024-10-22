import React from 'react';
import { useContent } from 'store/hooks';
import { Checkbox, CheckboxSize, Col, Row } from 'tno-core';

export const SearchInGroup: React.FC = () => {
  const [
    {
      search: { filter },
    },
    { storeSearchFilter: storeFilter },
  ] = useContent();

  return (
    <Col>
      <label className="search-in-label">Search in: </label>
      <Row className="options expanded space-top">
        <Checkbox
          id="chkInHeadline"
          label="Headline"
          checked={filter.inHeadline}
          checkboxSize={CheckboxSize.small}
          onChange={(e) => {
            storeFilter({ ...filter, inHeadline: e.target.checked });
          }}
        />
        <Checkbox
          id="chkInByline"
          label="Byline"
          checked={filter.inByline}
          checkboxSize={CheckboxSize.small}
          onChange={(e) => storeFilter({ ...filter, inByline: e.target.checked })}
        />
        <Checkbox
          id="chkInStory"
          label="Story text"
          checked={filter.inStory}
          checkboxSize={CheckboxSize.small}
          onChange={(e) => storeFilter({ ...filter, inStory: e.target.checked })}
        />
        <Checkbox
          id="chkInProgram"
          label="Program/Show"
          checked={filter.inProgram}
          checkboxSize={CheckboxSize.small}
          onChange={(e) => storeFilter({ ...filter, inProgram: e.target.checked })}
        />
      </Row>
    </Col>
  );
};
