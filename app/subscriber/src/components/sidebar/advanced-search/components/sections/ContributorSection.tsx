import { useMemo } from 'react';
import { useContent, useLookup, useLookupOptions } from 'store/hooks';
import { FieldSize, OptionItem, Row, Select } from 'tno-core';

/** Section for advanced filter that allows users to select contributors they want to filter content off of. */
export const ContributorSection: React.FC = () => {
  const [{ contributors }] = useLookup();
  const [{ searchFilter: filter }, { storeSearchFilter: storeFilter }] = useContent();
  const contributorOptions = useMemo(
    () =>
      contributors.map((c) => {
        return { value: c.id, label: c.name };
      }),
    [contributors],
  );
  return (
    <Row justifyContent="center">
      <Select
        name="contributorIds"
        isMulti
        key={filter.contributorIds?.join(',')}
        width={FieldSize.Stretch}
        className="contributors"
        options={contributorOptions}
        defaultValue={contributorOptions.filter((o) => {
          return filter.contributorIds?.includes(o.value);
        })}
        onChange={(newValue: any) => {
          const contributorIds = newValue.map((v: OptionItem) => v.value);
          storeFilter({ ...filter, contributorIds: contributorIds });
        }}
      />
    </Row>
  );
};
