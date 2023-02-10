import { ToggleGroup } from 'components/toggle-group';
import { ToolBarSection } from 'components/tool-bar';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { useLookupOptions } from 'hooks';
import React from 'react';
import { FaClock, FaFilter, FaIcons, FaUsers } from 'react-icons/fa';
import { useApp, useContent } from 'store/hooks';
import { filterEnabled } from 'store/hooks/lookup/utils';
import { Col, FieldSize, fromQueryString, IOptionItem, OptionItem, Row, Select } from 'tno-core';
import { getUserOptions } from 'utils';

import { DateRangeSection } from '.';
import { InputOption } from './InputOption';

export interface IFilterContentSectionProps {
  onChange: (filter: IContentListFilter) => void;
  onAdvancedFilterChange: (filter: IContentListAdvancedFilter) => void;
  onSearch: (filter: IContentListFilter & IContentListAdvancedFilter) => void;
}

/**
 * Component containing the filter section of the content tool bar
 * @param onChange determine what happens when filter changes are applied
 * @param onAdvancedFilterChange determine what happens when advanced filter changes are applied
 * @param onSearch determine what happens when the search button is clicked
 * @returns The filter content section
 */
export const FilterContentSection: React.FC<IFilterContentSectionProps> = ({
  onChange,
  onAdvancedFilterChange,
  onSearch,
}) => {
  const [{ filter, filterAdvanced }] = useContent();
  const [{ productOptions: pOptions, users }] = useLookupOptions();
  const [productOptions, setProductOptions] = React.useState<IOptionItem[]>([]);
  const [userOptions, setUserOptions] = React.useState<IOptionItem[]>([]);
  const [{ userInfo }] = useApp();
  const search = fromQueryString(window.location.search);

  const timeFrames = [
    { label: 'TODAY', value: 0 },
    { label: '24 HRS', value: 1 },
    { label: '48 HRS', value: 2 },
    { label: 'ALL', value: 3 },
  ];
  const timeFrameSelected = timeFrames[search.timeFrame ?? 0].label;

  const usersSelections = [
    { label: 'ALL CONTENT', value: 0 },
    { label: 'MY CONTENT', value: userInfo?.id ?? 0 },
  ];
  const usersSelected =
    usersSelections.find((i) => (i.value === search.userId ? +search.userId : 0))?.label ??
    'ALL CONTENT';

  React.useEffect(() => {
    setUserOptions(getUserOptions(users.filter((u) => !u.isSystemAccount)));
  }, [users]);

  React.useEffect(() => {
    setProductOptions([new OptionItem<number>('Any', 0), ...pOptions]);
  }, [pOptions]);

  // Change userId filter with value of dropdown
  const onOtherClick = (value?: number) => {
    value && onChange({ ...filter, userId: value });
  };

  /** clear time frame when start end date is selected */
  React.useEffect(() => {
    if ((!!filterAdvanced.startDate || !!filterAdvanced.endDate) && filter.timeFrame !== '') {
      onChange({ ...filter, timeFrame: '' });
    }
  }, [filterAdvanced.startDate, filterAdvanced.endDate, filter, onChange]);

  return (
    <ToolBarSection
      children={
        <Row>
          <Col>
            <Row>
              <FaClock className="icon-indicator" />
              <ToggleGroup
                defaultSelected={timeFrameSelected}
                disabled={!!filterAdvanced.startDate || !!filterAdvanced.endDate}
                options={[
                  {
                    label: 'TODAY',
                    onClick: () => {
                      onChange({ ...filter, timeFrame: 0 });
                    },
                  },
                  {
                    label: '24 HRS',
                    onClick: () => onChange({ ...filter, timeFrame: 1 }),
                  },
                  {
                    label: '48 HRS',
                    onClick: () => onChange({ ...filter, timeFrame: 2 }),
                  },
                  {
                    label: 'ALL',
                    onClick: () => onChange({ ...filter, timeFrame: 3 }),
                  },
                ]}
              />
            </Row>
            <Row>
              <FaUsers className="icon-indicator" />
              <ToggleGroup
                defaultSelected={usersSelected}
                options={[
                  {
                    label: 'ALL CONTENT',
                    onClick: () => onChange({ ...filter, userId: 0 }),
                  },
                  {
                    label: 'MY CONTENT',
                    onClick: () => onChange({ ...filter, userId: userInfo?.id ?? 0 }),
                  },
                  {
                    label: 'OTHER',
                    dropDownOptions: filterEnabled(userOptions),
                    onClick: onOtherClick,
                  },
                ]}
              />
            </Row>
          </Col>
          <Col>
            <DateRangeSection
              onChange={onChange}
              onAdvancedFilterChange={onAdvancedFilterChange}
              onSearch={onSearch}
            />
            <Row>
              <FaIcons className="icon-indicator" height="2em" width="2em" />
              <Select
                className="select"
                name="productIds"
                placeholder="Designation"
                isMulti
                closeMenuOnSelect={false}
                hideSelectedOptions={false}
                options={filterEnabled(productOptions)}
                value={
                  filter.productIds?.map((id) => productOptions.find((opt) => opt.value === id)) ??
                  ''
                }
                width={FieldSize.Big}
                defaultValue={productOptions[0]}
                components={{
                  Option: InputOption,
                }}
                onChange={(newValues) => {
                  const productIds = Array.isArray(newValues)
                    ? newValues.map((opt) => opt.value)
                    : [0];
                  onChange({
                    ...filter,
                    pageIndex: 0,
                    productIds: productIds,
                  });
                }}
              />
            </Row>
          </Col>
        </Row>
      }
      label="FILTER CONTENT"
      icon={<FaFilter />}
    />
  );
};
