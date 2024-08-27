import { IContentListFilter } from 'features/content/interfaces';
import React from 'react';
import { FaClock, FaFilter, FaIcons, FaUsers } from 'react-icons/fa';
import { useApp, useContent, useLookupOptions } from 'store/hooks';
import {
  Col,
  FieldSize,
  filterEnabledOptions,
  fromQueryString,
  getUserOptions,
  IOptionItem,
  replaceQueryParams,
  Row,
  Select,
  ToggleGroup,
  ToolBarSection,
} from 'tno-core';

import { DateRangeSection } from '.';
import { InputOption } from './InputOption';
import * as styled from './styled';

export interface IFilterContentSectionProps {}

/**
 * Component containing the filter section of the content tool bar
 * @param onChange determine what happens when filter changes are applied
 * @returns The filter content section
 */
export const FilterContentSection: React.FC<IFilterContentSectionProps> = () => {
  const [{ filter, filterAdvanced }, { storeFilter }] = useContent();
  const [{ mediaTypeOptions, users }] = useLookupOptions();
  const [userOptions, setUserOptions] = React.useState<IOptionItem[]>([]);
  const [{ userInfo }] = useApp();

  const search = fromQueryString(window.location.search);
  const timeFrames = [
    { label: 'TODAY', value: 0 },
    { label: '24 HRS', value: 1 },
    { label: '48 HRS', value: 2 },
    { label: 'ALL', value: 3 },
  ];
  const timeFrameSelected = timeFrames[search.timeFrame ?? filter.timeFrame]?.label ?? '';

  const usersSelections = [
    { label: 'ALL CONTENT', value: 0 },
    { label: 'MY CONTENT', value: userInfo?.id ?? 0 },
  ];
  const usersSelected =
    usersSelections.find((i) => (+i.value === +search.userId ? +search.userId : filter.userId))
      ?.label ?? 'ALL CONTENT';

  React.useEffect(() => {
    setUserOptions(getUserOptions(users.filter((u) => !u.isSystemAccount)));
  }, [users]);

  const onChange = React.useCallback(
    (filter: IContentListFilter) => {
      storeFilter(filter);
      replaceQueryParams(filter, { includeEmpty: false });
    },
    [storeFilter],
  );

  /** clear time frame when start end date is selected */
  React.useEffect(() => {
    if ((!!filterAdvanced.startDate || !!filterAdvanced.endDate) && filter.timeFrame !== '') {
      onChange({ ...filter, timeFrame: '' });
    }
  }, [filterAdvanced.startDate, filterAdvanced.endDate, filter, onChange]);

  // Change userId filter with value of dropdown
  const onOtherClick = React.useCallback(
    (filter: IContentListFilter, value?: number) => {
      value && onChange({ ...filter, userId: value });
    },
    [onChange],
  );

  return (
    <styled.FilterContentSection>
      <ToolBarSection
        children={
          <Row>
            <Col>
              <Row className="time-offset-filter">
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
              <Row className="content-owner-filter">
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
                      dropDownOptions: filterEnabledOptions(userOptions),
                      onClick: (e, option) =>
                        onOtherClick(filter, option?.value ? +option.value : undefined),
                    },
                  ]}
                />
              </Row>
            </Col>
            <Col>
              <DateRangeSection />
              <Row>
                <FaIcons className="icon-indicator" height="2em" width="2em" />
                <Select
                  className="select"
                  name="mediaTypeIds"
                  placeholder="Media Type"
                  isMulti
                  closeMenuOnSelect={false}
                  hideSelectedOptions={false}
                  options={filterEnabledOptions(mediaTypeOptions)}
                  value={
                    filter.mediaTypeIds?.map((id) =>
                      mediaTypeOptions.find((opt) => opt.value === id),
                    ) ?? ''
                  }
                  width={FieldSize.Big}
                  defaultValue={mediaTypeOptions[0]}
                  components={{
                    Option: InputOption,
                  }}
                  onChange={(newValues) => {
                    const mediaTypeIds = Array.isArray(newValues)
                      ? newValues.map((opt) => opt.value)
                      : [0];
                    onChange({
                      ...filter,
                      pageIndex: 0,
                      mediaTypeIds: mediaTypeIds,
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
    </styled.FilterContentSection>
  );
};
