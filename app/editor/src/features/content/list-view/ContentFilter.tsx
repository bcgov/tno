import { Checkbox, IOptionItem, OptionItem, RadioGroup, Select, SelectDate } from 'components/form';
import { useTooltips } from 'hooks';
import { ContentTypeName, IContentModel } from 'hooks/api-editor';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
import { filterEnabled } from 'store/hooks/lookup/utils';
import { initialContentState } from 'store/slices';
import { Button, ButtonVariant, FieldSize, Loader, Text } from 'tno-core';
import { Col, Row } from 'tno-core/dist/components/flex';
import { Page } from 'tno-core/dist/components/grid-table';
import { getSortableOptions, getUserOptions } from 'utils';

import { fieldTypes, timeFrames } from './constants';
import { IContentListFilter } from './interfaces';
import * as styled from './styled';

export interface IContentFilterProps {
  /** Function to call when the filter changes. */
  search: (filter: IContentListFilter) => Promise<Page<IContentModel>>;
}

/**
 * ContentFilter component provides a filter form to update the search results.
 * There are two types of filters (standard, advanced).
 * The standard filter will execute the 'search' function immediately on a change.
 * The advanced filter will execute the 'search' function only when requested by the user.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentFilter: React.FC<IContentFilterProps> = ({ search }) => {
  const [{ products, ingestTypes, users }] = useLookup();
  const [{ filter, filterAdvanced }, { storeFilter, storeFilterAdvanced }] = useContent();
  const [advancedHover, setAdvancedHover] = React.useState(false);
  useTooltips();

  const [productOptions, setProductOptions] = React.useState<IOptionItem[]>([]);
  const [userOptions, setUserOptions] = React.useState<IOptionItem[]>([]);
  const [timeFrame, setTimeFrame] = React.useState<IOptionItem>(
    timeFrames[Number(filter.timeFrame)],
  );

  React.useEffect(() => {
    setProductOptions(getSortableOptions(products, [new OptionItem<number>('Any', 0)]));
  }, [products]);

  React.useEffect(() => {
    setUserOptions(
      getUserOptions(
        users.filter((u) => !u.isSystemAccount),
        [new OptionItem<number>('All Users', 0)],
      ),
    );
  }, [users]);

  /** Handle enter key pressed for advanced filter */
  React.useEffect(() => {
    const keyDownHandler = (event: KeyboardEvent) => {
      if (event.key === 'Enter' && advancedHover) {
        event.preventDefault();
        search({ ...filter, pageIndex: 0, ...filterAdvanced });
      }
    };

    document.addEventListener('keydown', keyDownHandler);

    return () => {
      document.removeEventListener('keydown', keyDownHandler);
    };
  }, [filter, filterAdvanced, search, advancedHover]);

  // Convert the selected option into the value.
  const handleTimeChange: React.ChangeEventHandler<HTMLInputElement> = (
    e: React.ChangeEvent<HTMLInputElement>,
    values?: IOptionItem,
  ) => {
    const value = values ?? timeFrame;
    setTimeFrame(value);
    storeFilter({
      ...filter,
      pageIndex: 0,
      timeFrame: +`${value.value ?? 0}`,
    });
  };

  return (
    <styled.ContentFilter className="content-filter">
      <div>
        <Loader size="5em" visible={!users.length || !products.length || !ingestTypes.length} />
        <Select
          name="productId"
          label="Product Designation"
          options={productOptions}
          value={productOptions.find((mt) => mt.value === filter.productId)}
          defaultValue={productOptions[0]}
          onChange={(newValue) => {
            var productId = (newValue as IOptionItem).value ?? 0;
            storeFilter({
              ...filter,
              pageIndex: 0,
              productId: productId as number,
            });
          }}
        />
        <Select
          name="user"
          label="User"
          options={userOptions}
          value={userOptions.find((u) => u.value === filter.userId)}
          onChange={(newValue) => {
            var userId = (newValue as IOptionItem).value ?? '';
            storeFilter({
              ...filter,
              pageIndex: 0,
              userId: typeof userId === 'string' ? '' : userId,
            });
          }}
        />
        <RadioGroup
          name="timeFrame"
          label="Time Frame"
          direction="col-row"
          tooltip="Date created"
          value={timeFrame}
          options={timeFrames}
          onChange={handleTimeChange}
          disabled={!!filterAdvanced.startDate || !!filterAdvanced.endDate}
        />
        <div className="frm-in chg">
          <label>Filters</label>
          <div className="action-filters">
            <div>
              <Checkbox
                name="isPrintContent"
                label="Print Content"
                tooltip="Newspaper content without audio/video"
                value={filter.printContent}
                checked={filter.printContent}
                onChange={(e) => {
                  storeFilter({
                    ...filter,
                    pageIndex: 0,
                    contentType: e.target.checked ? ContentTypeName.PrintContent : undefined,
                  });
                }}
              />
              <Checkbox
                name="includedInCategory"
                label="Included in EoD"
                tooltip="Content included in Event of the Day"
                value={filter.includedInCategory}
                checked={filter.includedInCategory}
                onChange={(e) => {
                  storeFilter({
                    ...filter,
                    pageIndex: 0,
                    includedInCategory: e.target.checked,
                  });
                }}
              />
              <Checkbox
                name="ticker"
                label="On Ticker"
                value="On Ticker"
                tooltip="Content identified as on ticker"
                checked={filter.onTicker !== ''}
                onChange={(e) => {
                  storeFilter({
                    ...filter,
                    pageIndex: 0,
                    onTicker: e.target.checked ? e.target.value : '',
                  });
                }}
              />
            </div>
            <div>
              <Checkbox
                name="commentary"
                label="Commentary"
                value="Commentary"
                tooltip="Content identified as commentary"
                checked={filter.commentary !== ''}
                onChange={(e) => {
                  storeFilter({
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
                  storeFilter({
                    ...filter,
                    pageIndex: 0,
                    topStory: e.target.checked ? e.target.value : '',
                  });
                }}
              />
            </div>
          </div>
        </div>
      </div>
      <div
        className="box"
        onMouseOver={() => setAdvancedHover(true)}
        onMouseLeave={() => setAdvancedHover(false)}
      >
        <h2 className="caps">Advanced Search</h2>
        <div className="box-content">
          <Select
            name="fieldType"
            label="Field Type"
            options={fieldTypes}
            value={filterAdvanced.fieldType}
            width={FieldSize.Medium}
            onChange={(newValue) => {
              const value =
                newValue instanceof OptionItem ? newValue.toInterface() : (newValue as IOptionItem);
              storeFilterAdvanced({ ...filterAdvanced, fieldType: value });
            }}
          />
          <Text
            className="test"
            name="searchTerm"
            label="Search Terms"
            value={filterAdvanced.searchTerm}
            onChange={(e) => {
              storeFilterAdvanced({ ...filterAdvanced, searchTerm: e.target.value.trim() });
            }}
          ></Text>
        </div>
        <Col className="frm-in dateRange" alignItems="flex-start">
          <label data-for="main-tooltip" data-tip="Date created">
            Date Range
          </label>
          <Row>
            <SelectDate
              name="startDate"
              placeholderText="YYYY MM DD"
              selected={!!filterAdvanced.startDate ? new Date(filterAdvanced.startDate) : undefined}
              width={FieldSize.Small}
              onChange={(date) =>
                storeFilterAdvanced({
                  ...filterAdvanced,
                  startDate: !!date ? date.toString() : undefined,
                })
              }
            />
            <SelectDate
              name="endDate"
              placeholderText="YYYY MM DD"
              selected={!!filterAdvanced.endDate ? new Date(filterAdvanced.endDate) : undefined}
              width={FieldSize.Small}
              onChange={(date) => {
                date?.setHours(23, 59, 59);
                storeFilterAdvanced({
                  ...filterAdvanced,
                  endDate: !!date ? date.toString() : undefined,
                });
              }}
            />
          </Row>
        </Col>
        <Button
          name="search"
          onClick={() => search({ ...filter, pageIndex: 0, ...filterAdvanced })}
        >
          Search
        </Button>
        <Button
          name="clear"
          variant={ButtonVariant.secondary}
          onClick={() => {
            storeFilterAdvanced({
              ...initialContentState.filterAdvanced,
            });
            storeFilter({ ...filter, pageIndex: 0 });
          }}
        >
          Clear
        </Button>
      </div>
    </styled.ContentFilter>
  );
};
