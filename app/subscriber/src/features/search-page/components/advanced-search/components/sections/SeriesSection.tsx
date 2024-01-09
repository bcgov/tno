import { useMemo } from 'react';
import { useContent, useLookup } from 'store/hooks';
import { FieldSize, OptionItem, Row, Select } from 'tno-core';

/** allows user to filter based off of show/programs */
export const SeriesSection: React.FC = () => {
  const [
    {
      search: { filter },
    },
    { storeSearchFilter: storeFilter },
  ] = useContent();
  const [{ series }] = useLookup();
  const seriesOptions = useMemo(
    () =>
      series.map((s) => {
        return { value: s.id, label: s.name };
      }),
    [series],
  );
  return (
    <Row justifyContent="center">
      <Select
        width={FieldSize.Stretch}
        key={filter.seriesIds?.join(',')}
        isMulti
        options={seriesOptions}
        name="series"
        onChange={(newValue: any) => {
          const seriesIds = newValue.map((v: OptionItem<number>) => v.value);
          storeFilter({ ...filter, seriesIds: seriesIds });
        }}
        value={seriesOptions.filter((o) => o.value && filter.seriesIds?.includes(+o.value))}
      />
    </Row>
  );
};
