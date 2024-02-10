import { useContent, useLookupOptions } from 'store/hooks';
import { Button, ButtonVariant, Checkbox, Row, Select, Show } from 'tno-core';

import { IFilterDisplayProps } from './IFilterDisplayProps';

/** allows user to filter based off of show/programs */
export const TagSection: React.FC<IFilterDisplayProps> = ({ displayFiltersAsDropdown }) => {
  const [
    {
      search: { filter },
    },
    { storeSearchFilter: storeFilter },
  ] = useContent();
  const [{ tags }] = useLookupOptions();

  const tagOptions = tags
    .filter((t) => t.isEnabled)
    .map((t) => {
      return { value: t.code, label: `${t.name} [${t.code}]` };
    });

  return (
    <Row justifyContent="center">
      <Show visible={!displayFiltersAsDropdown}>
        <div className="check-box-list">
          <div className="chk-box-container chk-tag select-all">
            <Button
              variant={ButtonVariant.link}
              onClick={() => storeFilter({ ...filter, tags: tagOptions.map((m) => m.value!) })}
            >
              Select All
            </Button>
            /
            <Button
              variant={ButtonVariant.link}
              onClick={() => storeFilter({ ...filter, tags: [] })}
            >
              Deselect All
            </Button>
          </div>
          {tagOptions.map((item, index) => (
            <div key={`chk-tag-${index}`} className="chk-box-container chk-tag">
              <Checkbox
                id={`chk-tag-${index}`}
                label={item.label}
                checked={filter.tags?.includes(item.value)}
                value={item.value}
                onChange={(e) => {
                  storeFilter({
                    ...filter,
                    tags: e.target.checked
                      ? [...filter.tags!, e.target.value] // add it
                      : filter.tags?.filter((i) => i !== e.target.value), // remove it
                  });
                }}
              />
            </div>
          ))}
        </div>
      </Show>
      <Show visible={displayFiltersAsDropdown}>
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
          value={tagOptions.filter((o) => o.value && filter.tags?.includes(o.value))}
        />
      </Show>
    </Row>
  );
};
