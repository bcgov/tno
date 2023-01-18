import { ContentTypeName, useLookupOptions } from 'hooks';
import React from 'react';
import { useContent } from 'store/hooks';
import { filterEnabled } from 'store/hooks/lookup/utils';
import {
  Checkbox,
  Col,
  FieldSize,
  IOptionItem,
  OptionItem,
  RadioGroup,
  replaceQueryParams,
  Row,
  Select,
} from 'tno-core';

import { timeFrames } from '../list-view/constants';
import { queryToFilter } from '../list-view/utils';
import { IMorningReportFilter } from './interfaces';
import * as styled from './styled';

export interface IMorningReportFilterProps {}

export const MorningReportFilter: React.FC<IMorningReportFilterProps> = () => {
  const [{ morningReportFilter: filter }, { storeMorningReportFilter }] = useContent();
  const [{ productOptions: pOptions, sourceOptions }] = useLookupOptions();

  const [productOptions, setProductOptions] = React.useState<IOptionItem[]>([]);

  const timeFrame = timeFrames[Number(filter.timeFrame)];

  React.useEffect(() => {
    // Extract query string values and place them into redux store.
    storeMorningReportFilter(
      queryToFilter(
        { ...filter, contentType: ContentTypeName.PrintContent },
        window.location.search,
      ),
    );
    // Only want this to run on the first load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    setProductOptions([new OptionItem<number>('Any', 0), ...pOptions]);
  }, [pOptions]);

  const onFilterChange = (filter: IMorningReportFilter) => {
    storeMorningReportFilter(filter);
    replaceQueryParams(filter, { includeEmpty: false });
  };

  // Convert the selected option into the value.
  const handleTimeChange: React.ChangeEventHandler<HTMLInputElement> = (
    e: React.ChangeEvent<HTMLInputElement>,
    values?: IOptionItem,
  ) => {
    const value = values ?? timeFrame;
    onFilterChange({
      ...filter,
      pageIndex: 0,
      timeFrame: +`${value.value ?? 0}`,
    });
  };

  return (
    <styled.MorningReportFilter>
      <Row>
        <Select
          name="productId"
          label="Product Designation"
          width={FieldSize.Medium}
          options={filterEnabled(productOptions)}
          value={productOptions.find((mt) => mt.value === filter.productId)}
          defaultValue={productOptions[0]}
          onChange={(newValue) => {
            var productId = !!newValue ? (newValue as IOptionItem).value ?? 0 : 0;
            onFilterChange({
              ...filter,
              pageIndex: 0,
              productId: productId as number,
            });
          }}
        />
        <Select
          name="sourceId"
          label="Source"
          width={FieldSize.Medium}
          onChange={(newValue: any) => {
            var sourceId = !!newValue ? (newValue as IOptionItem).value ?? 0 : 0;
            onFilterChange({
              ...filter,
              pageIndex: 0,
              sourceId: sourceId as number,
            });
          }}
          options={[new OptionItem('', 0) as IOptionItem].concat([...filterEnabled(sourceOptions)])}
          value={sourceOptions.find((s) => s.label === filter.otherSource)}
        />
        <RadioGroup
          name="timeFrame"
          label="Time Frame"
          direction="col-row"
          tooltip="Date published"
          value={timeFrame}
          options={timeFrames}
          onChange={handleTimeChange}
        />
        <Col className="frm-in chg">
          <label>Filters</label>
          <Row className="action-filters">
            <Checkbox
              name="frontPage"
              label="Front Page"
              value="Front Page"
              tooltip="Front page content"
              checked={filter.productId === 11}
              onChange={(e) => {
                onFilterChange({
                  ...filter,
                  pageIndex: 0,
                  productId: e.target.checked ? 11 : 0,
                });
              }}
            />
            <Checkbox
              name="commentary"
              label="Commentary"
              value="Commentary"
              tooltip="Content identified as commentary"
              checked={filter.commentary !== ''}
              onChange={(e) => {
                onFilterChange({
                  ...filter,
                  pageIndex: 0,
                  commentary: e.target.checked ? e.target.value : '',
                });
              }}
            />
            <Checkbox
              name="topStory"
              label="Top Story"
              value="Top Story"
              tooltip="Content identified as a top story"
              checked={filter.topStory !== ''}
              onChange={(e) => {
                onFilterChange({
                  ...filter,
                  pageIndex: 0,
                  topStory: e.target.checked ? e.target.value : '',
                });
              }}
            />
            <Checkbox
              name="includeHidden"
              label="Hidden"
              tooltip="Content removed from the list"
              value={filter.includeHidden}
              checked={filter.includeHidden}
              onChange={(e) => {
                onFilterChange({
                  ...filter,
                  pageIndex: 0,
                  includeHidden: e.target.checked,
                });
              }}
            />
          </Row>
        </Col>
      </Row>
    </styled.MorningReportFilter>
  );
};
