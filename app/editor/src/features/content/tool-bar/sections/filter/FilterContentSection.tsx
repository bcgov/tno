import { IOptionItem, OptionItem, Select, SelectDate } from 'components/form';
import { ToggleGroup } from 'components/toggle-group';
import { ToolBarSection } from 'components/tool-bar';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { useLookupOptions } from 'hooks';
import React from 'react';
import { FaCalendarAlt, FaClock, FaFilter, FaIcons, FaUsers } from 'react-icons/fa';
import { useApp, useContent } from 'store/hooks';
import { Col, FieldSize, fromQueryString, Row } from 'tno-core';
import { getUserOptions } from 'utils';

import { InputOption } from './InputOption';

export interface IFilterContentSectionProps {
  onChange: (filter: IContentListFilter) => void;
  onAdvancedFilterChange: (filter: IContentListAdvancedFilter) => void;
}

/**
 * Component containing the filter section of the content tool bar
 * @param onChange determine what happens when filter changes are applied
 * @returns The filter content section
 */
export const FilterContentSection: React.FC<IFilterContentSectionProps> = ({
  onChange,
  onAdvancedFilterChange,
}) => {
  const [{ filter, filterAdvanced }] = useContent();
  const [{ productOptions: pOptions, users }] = useLookupOptions();
  const [productOptions, setProductOptions] = React.useState<IOptionItem[]>([]);
  const [userOptions, setUserOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    setUserOptions(getUserOptions(users.filter((u) => !u.isSystemAccount)));
  }, [users]);

  React.useEffect(() => {
    setProductOptions([new OptionItem<number>('Any', 0), ...pOptions]);
  }, [pOptions]);

  const [{ userInfo }] = useApp();

  // Change userId filter with value of dropdown
  const onOtherClick = (value?: number) => {
    value && onChange({ ...filter, userId: value });
  };

  const search = fromQueryString(window.location.search);
  const timeFrames = [
    { label: 'TODAY', value: 0 },
    { label: '24 HRS', value: 1 },
    { label: '48 HRS', value: 2 },
    { label: 'ALL', value: 3 },
  ];
  const usersSelections = [
    { label: 'ALL CONTENT', value: 0 },
    { label: 'MY CONTENT', value: userInfo?.id },
  ];

  return (
    <ToolBarSection
      children={
        <Row>
          <Col>
            <Row>
              <FaClock className="icon-indicator" />
              <ToggleGroup
                defaultSelected={timeFrames[search.timeFrame].label.toLowerCase() ?? 'today'}
                options={[
                  {
                    label: 'TODAY',
                    onClick: () => {
                      onChange({ ...filter, timeFrame: 0 });
                    },
                  },
                  { label: '24 HRS', onClick: () => onChange({ ...filter, timeFrame: 1 }) },
                  { label: '48 HRS', onClick: () => onChange({ ...filter, timeFrame: 2 }) },
                  { label: 'ALL', onClick: () => onChange({ ...filter, timeFrame: 3 }) },
                ]}
              />
            </Row>
            <Row>
              <FaUsers className="icon-indicator" />
              <ToggleGroup
                defaultSelected={usersSelections[search.userId].label.toLowerCase() ?? 'my content'}
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
                    dropDownOptions: userOptions,
                    onClick: onOtherClick,
                  },
                ]}
              />
            </Row>
          </Col>
          <Col>
            <Row className="date-range">
              <FaCalendarAlt className="action-icon" />

              <SelectDate
                name="startDate"
                placeholderText="mm/dd/yyyy"
                selected={
                  !!filterAdvanced.startDate ? new Date(filterAdvanced.startDate) : undefined
                }
                width="8em"
                onChange={(date) =>
                  onAdvancedFilterChange({
                    ...filterAdvanced,
                    startDate: !!date ? date.toString() : undefined,
                  })
                }
              />
              <SelectDate
                name="endDate"
                placeholderText="mm/dd/yyyy"
                selected={!!filterAdvanced.endDate ? new Date(filterAdvanced.endDate) : undefined}
                width="8em"
                onChange={(date) => {
                  date?.setHours(23, 59, 59);
                  onAdvancedFilterChange({
                    ...filterAdvanced,
                    endDate: !!date ? date.toString() : undefined,
                  });
                }}
              />
            </Row>
            <Row>
              <FaIcons className="icon-indicator" height="2em" width="2em" />
              <Select
                className="select"
                name="productIds"
                placeholder="Designation"
                isMulti
                closeMenuOnSelect={false}
                hideSelectedOptions={false}
                options={productOptions}
                // value={productOptions.find((mt) => mt.value === filter.productId)}
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
