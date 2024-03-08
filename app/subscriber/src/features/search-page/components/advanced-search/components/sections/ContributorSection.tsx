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

/** Section for advanced filter that allows users to select contributors they want to filter content off of. */
export const ContributorSection: React.FC<IFilterDisplayProps> = ({ displayFiltersAsDropdown }) => {
  const [{ contributors }] = useLookup();
  const [{ search }, { storeSearchFilter: storeFilter }] = useContent();
  const filter = search.filter;
  const contributorOptions = useMemo(
    () =>
      contributors
        .filter((c) => c.isEnabled)
        .map((c) => {
          return { value: c.id, label: c.name };
        }),
    [contributors],
  );
  return (
    <Row justifyContent="center">
      <Show visible={!displayFiltersAsDropdown}>
        <div className="check-box-list">
          <div className="chk-box-container chk-contributor select-all">
            <Button
              variant={ButtonVariant.link}
              onClick={() =>
                storeFilter({ ...filter, contributorIds: contributorOptions.map((m) => +m.value!) })
              }
            >
              Select All
            </Button>
            /
            <Button
              variant={ButtonVariant.link}
              onClick={() => storeFilter({ ...filter, contributorIds: [] })}
            >
              Deselect All
            </Button>
          </div>
          {contributorOptions.map((item, index) => (
            <div key={`chk-contributor-${index}`} className="chk-box-container chk-contributor">
              <Checkbox
                id={`chk-contributor-${index}`}
                label={item.label}
                checked={filter.contributorIds?.includes(+item.value!)}
                value={item.value}
                onChange={(e) => {
                  storeFilter({
                    ...filter,
                    contributorIds: e.target.checked
                      ? [...(filter.contributorIds ?? []), +e.target.value] // add it
                      : filter.contributorIds?.filter((i) => i !== +e.target.value), // remove it
                  });
                }}
              />
            </div>
          ))}
        </div>
      </Show>
      <Show visible={displayFiltersAsDropdown}>
        <Select
          name="contributorIds"
          isMulti
          key={search.filter.contributorIds?.join(',')}
          width={FieldSize.Stretch}
          className="contributors"
          options={contributorOptions}
          onChange={(newValue: any) => {
            const contributorIds = newValue.map((v: OptionItem) => v.value);
            storeFilter({ ...filter, contributorIds: contributorIds });
          }}
          value={contributorOptions.filter(
            (o) => o.value && filter.contributorIds?.includes(+o.value),
          )}
        />
      </Show>
    </Row>
  );
};
