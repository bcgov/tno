import { FieldSize, Row, Text } from 'tno-core';

import { IExpandedSectionProps } from '../../interfaces';

/** Section for advanced filter that allows users to select paper attributes they want to filter content off of. */
export const PaperSection: React.FC<IExpandedSectionProps> = ({
  setAdvancedSearch,
  advancedSearch,
}) => {
  return (
    <Row className="paper-attributes-container" justifyContent="center">
      <Text
        placeholder="Section"
        name="section"
        width={FieldSize.Tiny}
        onChange={(e) => setAdvancedSearch({ ...advancedSearch, section: e.target.value })}
      />
      <Text
        placeholder="Page"
        name="page"
        width={FieldSize.Tiny}
        onChange={(e) => setAdvancedSearch({ ...advancedSearch, page: e.target.value })}
      />
      <Text
        placeholder="Edition"
        name="edition"
        width={FieldSize.Tiny}
        onChange={(e) => setAdvancedSearch({ ...advancedSearch, edition: e.target.value })}
      />
    </Row>
  );
};
