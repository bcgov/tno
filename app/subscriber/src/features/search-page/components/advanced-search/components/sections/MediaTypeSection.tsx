import React from 'react';
import { useApp, useContent, useLookupOptions } from 'store/hooks';
import { Button, ButtonVariant, Checkbox, FieldSize, Row, Select, Show } from 'tno-core';

import { IFilterDisplayProps } from './IFilterDisplayProps';

/** component that allows user to filter down the advanced search based on products */
export const MediaTypeSection: React.FC<IFilterDisplayProps> = ({ displayFiltersAsDropdown }) => {
  const [
    {
      search: { filter },
    },
    { storeSearchFilter: storeFilter },
  ] = useContent();
  const [{ userInfo }] = useApp();
  const [{ mediaTypeOptions }] = useLookupOptions();

  return (
    <Row justifyContent="center">
      <Show visible={!displayFiltersAsDropdown}>
        <div className="check-box-list">
          <div className="chk-box-container chk-media-type select-all">
            <Button
              variant={ButtonVariant.link}
              onClick={() =>
                storeFilter({ ...filter, mediaTypeIds: mediaTypeOptions.map((m) => +m.value!) })
              }
            >
              Select All
            </Button>
            /
            <Button
              variant={ButtonVariant.link}
              onClick={() => storeFilter({ ...filter, mediaTypeIds: [] })}
            >
              Deselect All
            </Button>
          </div>
          {mediaTypeOptions
            .filter((mt) => !userInfo?.mediaTypes.includes(+mt.value!))
            .map((item, index) => (
              <div key={`chk-media-type-${index}`} className="chk-box-container chk-media-type">
                <Checkbox
                  id={`chk-media-type-${index}`}
                  label={item.label}
                  checked={filter.mediaTypeIds?.includes(+item.value!)}
                  value={item.value}
                  onChange={(e) => {
                    storeFilter({
                      ...filter,
                      mediaTypeIds: e.target.checked
                        ? [...(filter.mediaTypeIds ?? []), +e.target.value] // add it
                        : filter.mediaTypeIds?.filter((i) => i !== +e.target.value), // remove it
                    });
                  }}
                />
              </div>
            ))}
        </div>
      </Show>
      <Show visible={displayFiltersAsDropdown}>
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
      </Show>
    </Row>
  );
};
