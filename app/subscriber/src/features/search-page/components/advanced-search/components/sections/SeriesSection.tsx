import { useMemo } from 'react';
import { useContent, useLookup } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Checkbox,
  FieldSize,
  OptionItem,
  Row,
  Select,
  Show,
} from 'tno-core';

import { IFilterDisplayProps } from './IFilterDisplayProps';

/** allows user to filter based off of show/programs */
export const SeriesSection: React.FC<IFilterDisplayProps> = ({ displayFiltersAsDropdown }) => {
  const [
    {
      search: { filter },
    },
    { storeSearchFilter: storeFilter },
  ] = useContent();
  const [{ series }] = useLookup();
  const seriesOptions = useMemo(
    () =>
      series
        .filter((f) => f.isEnabled)
        .map((s) => {
          return { value: s.id, label: s.name };
        }),
    [series],
  );
  return (
    <Row justifyContent="center">
      <Show visible={!displayFiltersAsDropdown}>
        <div className="check-box-list">
          <div className="chk-box-container chk-series select-all">
            <Button
              variant={ButtonVariant.link}
              onClick={() =>
                storeFilter({ ...filter, seriesIds: seriesOptions.map((m) => +m.value!) })
              }
            >
              Select All
            </Button>
            /
            <Button
              variant={ButtonVariant.link}
              onClick={() => storeFilter({ ...filter, seriesIds: [] })}
            >
              Deselect All
            </Button>
          </div>
          {seriesOptions.map((item, index) => (
            <div key={`chk-series-${index}`} className="chk-box-container chk-series">
              <Checkbox
                id={`chk-series-${index}`}
                label={item.label}
                checked={filter.seriesIds?.includes(+item.value!)}
                value={item.value}
                onChange={(e) => {
                  storeFilter({
                    ...filter,
                    seriesIds: e.target.checked
                      ? [...(filter.seriesIds ?? []), +e.target.value] // add it
                      : filter.seriesIds?.filter((i) => i !== +e.target.value), // remove it
                  });
                }}
              />
            </div>
          ))}
        </div>
      </Show>
      <Show visible={displayFiltersAsDropdown}>
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
      </Show>
    </Row>
  );
};
