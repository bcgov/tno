import { ContentTypeName, Row, Select } from 'tno-core';

import { IExpandedSectionProps } from '../../interfaces';

/** section that allows you to filter down based on content type with a drop down menu */
export const ContentSection: React.FC<IExpandedSectionProps> = ({
  setAdvancedSearch,
  advancedSearch,
}) => {
  return (
    <Row className="content-types-container" justifyContent="center">
      <Select
        name="content-types"
        isMulti
        className="content-types centered"
        width="25em"
        options={[
          { label: 'Audio/Video', value: ContentTypeName.AudioVideo },
          { label: 'Print Content', value: ContentTypeName.PrintContent },
          { label: 'Image', value: ContentTypeName.Image },
          { label: 'Story', value: ContentTypeName.Story },
        ]}
        onChange={(newValues) => {
          Array.isArray(newValues) &&
            setAdvancedSearch({
              ...advancedSearch,
              contentTypes: newValues.map((v) => v.value),
            });
        }}
      />
    </Row>
  );
};
