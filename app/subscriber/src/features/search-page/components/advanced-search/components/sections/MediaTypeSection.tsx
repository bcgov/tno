import { useContent, useLookupOptions } from 'store/hooks';
import { FieldSize, Row, Select } from 'tno-core';

/** component that allows user to filter down the advanced search based on products */
export const MediaTypeSection: React.FC = () => {
  const [
    {
      search: { filter },
    },
    { storeSearchFilter: storeFilter },
  ] = useContent();
  const [{ mediaTypeOptions }] = useLookupOptions();

  return (
    <Row justifyContent="center">
      <Select
        name="productIds"
        isMulti
        width={FieldSize.Stretch}
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
        value={mediaTypeOptions.filter((o) => o.value && filter.mediaTypeIds?.includes(+o.value))}
      />
    </Row>
  );
};
