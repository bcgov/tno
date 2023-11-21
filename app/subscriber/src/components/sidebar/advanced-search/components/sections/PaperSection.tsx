import { useContent } from 'store/hooks';
import { FieldSize, Row, Text } from 'tno-core';

/** Section for advanced filter that allows users to select paper attributes they want to filter content off of. */
export const PaperSection: React.FC = () => {
  const [{ searchFilter: filter }, { storeSearchFilter: storeFilter }] = useContent();
  return (
    <Row className="paper-attributes-container" justifyContent="center">
      <Text
        placeholder="Section"
        name="section"
        width={FieldSize.Tiny}
        value={filter.section}
        onChange={(e) => storeFilter({ ...filter, section: e.target.value })}
      />
      <Text
        placeholder="Page"
        name="page"
        value={filter.page}
        width={FieldSize.Tiny}
        onChange={(e) => storeFilter({ ...filter, page: e.target.value })}
      />
      <Text
        placeholder="Edition"
        name="edition"
        value={filter.edition}
        width={FieldSize.Tiny}
        onChange={(e) => storeFilter({ ...filter, edition: e.target.value })}
      />
    </Row>
  );
};
