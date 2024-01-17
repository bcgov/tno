import { useContent, useLookupOptions } from 'store/hooks';
import { Checkbox, FieldSize, Row, Select } from 'tno-core';

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
      <div className='check-box-list'>
        <div className='chk-media-type select-all'>
          <Checkbox 
            id='check-media-type-all'
            label='Select all'
            checked={filter.mediaTypeIds?.length == mediaTypeOptions.length} onChange={(e) => {
              storeFilter({
                ...filter,
                mediaTypeIds: mediaTypeOptions.map((m) => +m.value!)
              });
            }} />
        </div>
        {mediaTypeOptions.map((item, index) => (
          <div key={index} className='chk-media-type'>
            <Checkbox
              id={`chk-media-type-${index}`}
              key={index}
              label={item.label}
              checked={filter.mediaTypeIds?.includes(+item.value!)}
              value={item.value}
              onChange={(e) => {
                storeFilter({
                  ...filter,
                  mediaTypeIds: e.target.checked
                    ? [...filter.mediaTypeIds!, +e.target.value] // add it
                    : filter.mediaTypeIds?.filter((i) => i !== +e.target.value), // remove it
                });
              }}
            />
          </div>
        ))}
      </div>
    </Row>
  );
};
