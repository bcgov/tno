import { useLookupOptions } from 'store/hooks';
import { OptionItem, Row, Select } from 'tno-core';

import { IExpandedSectionProps } from '../../interfaces';

/** Section for advanced filter that allows users to select contributors they want to filter content off of. */
export const ContributorSection: React.FC<IExpandedSectionProps> = ({
  setAdvancedSearch,
  advancedSearch,
}) => {
  const [{ contributorOptions }] = useLookupOptions();
  return (
    <Row justifyContent="center">
      <Select
        name="contributorIds"
        isMulti
        width="25em"
        className="contributors"
        options={contributorOptions}
        // value={
        //   contributorOptions.filter((mt) =>
        //     values.settings.contributorIds?.some((p: number) => p === mt.value),
        //   ) ?? []
        // }
        onChange={(newValue: any) => {
          const contributorIds = newValue.map((v: OptionItem) => v.value);
          setAdvancedSearch({ ...advancedSearch, contributorIds: contributorIds });
        }}
      />
    </Row>
  );
};
