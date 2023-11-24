import { useContent, useLookupOptions } from 'store/hooks';
import { Row, Select } from 'tno-core';

/** allows user to filter based off of show/programs */
export const TagSection: React.FC = () => {
  const [{ searchFilter: filter }, { storeSearchFilter: storeFilter }] = useContent();
  const [{ tags }] = useLookupOptions();
  const tagOptions = tags.map((t) => {
    return { value: t.code, label: t.name };
  });
  return (
    <Row justifyContent="center">
      <Select
        width="25em"
        isMulti
        key={filter.tags?.join(',')}
        options={tagOptions}
        name="series"
        onChange={(newValues) => {
          Array.isArray(newValues) &&
            storeFilter({
              ...filter,
              tags: newValues.map((v) => v.value),
            });
        }}
        defaultValue={tagOptions.filter((o) => {
          return filter.tags?.includes(o.value);
        })}
      />
    </Row>
  );
};
