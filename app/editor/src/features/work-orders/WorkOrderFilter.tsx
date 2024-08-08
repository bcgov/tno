import React from 'react';
import { useLookup } from 'store/hooks';
import {
  Checkbox,
  IconButton,
  IWorkOrderFilter,
  Row,
  Settings,
  Show,
  Text,
  WorkOrderStatusName,
} from 'tno-core';

interface IWorkOrderFilterProps {
  filter: IWorkOrderFilter;
  onFilterChange: (filter: IWorkOrderFilter) => void;
}

export const WorkOrderFilter = ({ filter, onFilterChange }: IWorkOrderFilterProps) => {
  const [keywords, setKeywords] = React.useState<string>('');
  const [{ settings }] = useLookup();

  // Settings contains a link to the news radio media types.
  const newsRadioMediaTypeFilter = (
    settings.find((s) => s.name === Settings.NewsRadioMediaTypeFilter)?.value ?? ''
  )
    .split(',')
    .filter((v) => v)
    .map((v) => +v);

  return (
    <Row className="filter" justifyContent="center" alignItems="center" gap="1rem">
      <Checkbox
        name="ready"
        label="Ready for review"
        checked={filter.status?.includes(WorkOrderStatusName.Completed) === true}
        onChange={(e) =>
          onFilterChange({
            ...filter,
            isApproved: e.target.checked ? false : undefined,
            status: e.target.checked ? [WorkOrderStatusName.Completed] : undefined,
          })
        }
      />
      <Checkbox
        name="inProgress"
        label="In progress"
        checked={filter.status?.includes(WorkOrderStatusName.InProgress) === true}
        onChange={(e) =>
          onFilterChange({
            ...filter,
            isApproved: undefined,
            status: e.target.checked
              ? [WorkOrderStatusName.Submitted, WorkOrderStatusName.InProgress]
              : undefined,
          })
        }
      />
      <Checkbox
        name="failed"
        label="Failed"
        checked={filter.status?.includes(WorkOrderStatusName.Failed) === true}
        onChange={(e) =>
          onFilterChange({
            ...filter,
            isApproved: undefined,
            status: e.target.checked ? [WorkOrderStatusName.Failed] : undefined,
          })
        }
      />
      <Show visible={!!newsRadioMediaTypeFilter.length}>
        <Checkbox
          name="newsRadio"
          label="News Radio"
          checked={!!filter.mediaTypeIds?.length}
          onChange={(e) =>
            onFilterChange({
              ...filter,
              isApproved: undefined,
              mediaTypeIds: e.target.checked ? newsRadioMediaTypeFilter : undefined,
            })
          }
        />
      </Show>
      <Text
        name="search"
        placeholder="Search by keyword"
        onChange={(e) => {
          setKeywords(e.target.value);
        }}
        onKeyDown={(e) => {
          const value = (e.target as any).value;
          if (e.code === 'Enter')
            onFilterChange({ ...filter, keywords: value !== '' ? value : undefined });
        }}
        value={keywords}
      >
        <Row gap="0.25rem" nowrap>
          <IconButton
            iconType="search"
            title="Search"
            onClick={() => {
              onFilterChange({ ...filter, keywords: keywords !== '' ? keywords : undefined });
            }}
          />
          <IconButton
            iconType="reset"
            title="Clear"
            onClick={() => {
              setKeywords('');
              onFilterChange({ ...filter, keywords: undefined });
            }}
          />
        </Row>
      </Text>
    </Row>
  );
};
