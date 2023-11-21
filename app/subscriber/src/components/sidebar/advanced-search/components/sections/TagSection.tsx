import { useLookupOptions } from 'store/hooks';
import { Row, Select } from 'tno-core';

import { IExpandedSectionProps } from '../../interfaces';

/** allows user to filter based off of show/programs */
export const TagSection: React.FC<IExpandedSectionProps> = ({
  advancedSearch,
  setAdvancedSearch,
}) => {
  const [{ tags }] = useLookupOptions();
  return (
    <Row justifyContent="center">
      <Select
        width="25em"
        isMulti
        options={tags.map((t) => {
          return { value: t.code, label: t.name };
        })}
        name="series"
        onChange={(newValues) => {
          Array.isArray(newValues) &&
            setAdvancedSearch({
              ...advancedSearch,
              tags: newValues.map((v) => v.value),
            });
        }}
      />
    </Row>
  );
};
