import { useMemo } from 'react';
import { useContent, useLookupOptions } from 'store/hooks';
import { Row, Select } from 'tno-core';

/** component that allows user to filter down the advanced search based on products */
export const MediaTypeSection: React.FC = () => {
  const [{ searchFilter: filter }, { storeSearchFilter: storeFilter }] = useContent();
  const [{ mediaTypes }] = useLookupOptions();

  const mediaTypeOptions = useMemo(
    () =>
      mediaTypes.map((t) => {
        return { value: t.id, label: t.name };
      }),
    [mediaTypes],
  );

  return (
    <Row justifyContent="center">
      <Select
        name="productIds"
        isMulti
        width="25em"
        key={filter.mediaTypeIds?.join(',')}
        className="products"
        onChange={(newValues) => {
          Array.isArray(newValues) &&
            storeFilter({
              ...filter,
              mediaTypeIds: newValues.map((v) => v.value),
            });
        }}
        options={mediaTypeOptions}
        defaultValue={mediaTypeOptions.filter((o) => {
          return filter.mediaTypeIds?.includes(o.value);
        })}
      />
    </Row>
  );
};
