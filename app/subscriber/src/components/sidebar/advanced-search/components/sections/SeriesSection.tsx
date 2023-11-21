import { useLookupOptions } from 'store/hooks';
import { OptionItem, Row, Select } from 'tno-core';

import { IExpandedSectionProps } from '../../interfaces';

/** allows user to filter based off of show/programs */
export const SeriesSection: React.FC<IExpandedSectionProps> = ({
  advancedSearch,
  setAdvancedSearch,
}) => {
  const [{ seriesOptions }] = useLookupOptions();
  return (
    <Row justifyContent="center">
      <Select
        width="25em"
        isMulti
        options={seriesOptions}
        name="series"
        onChange={(newValue: any) => {
          const seriesIds = newValue.map((v: OptionItem) => v.value);
          setAdvancedSearch({ ...advancedSearch, contributorIds: seriesIds });
        }}
      />
    </Row>
  );
};
