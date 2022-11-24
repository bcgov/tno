import {
  Checkbox,
  instanceOfIOption,
  IOptionItem,
  OptionItem,
  RadioGroup,
  Select,
  SelectDate,
} from 'components/form';
import { useLookupOptions, useTooltips } from 'hooks';
import { ContentTypeName } from 'hooks/api-editor';
import React from 'react';
import { useContent } from 'store/hooks';
import { initialContentState } from 'store/slices';
import { Button, ButtonVariant, FieldSize, Loader, replaceQueryParams, Show, Text } from 'tno-core';
import { Col, Row } from 'tno-core/dist/components/flex';
import { getUserOptions } from 'utils';

import { fieldTypes, timeFrames } from './constants';
import { IContentListAdvancedFilter, IContentListFilter } from './interfaces';
import * as styled from './styled';
import { queryToFilter, queryToFilterAdvanced } from './utils';

export interface IContentFilterProps {
  /**
   * Inform the parent of the request to search.
   */
  onSearch: (filter: IContentListFilter & IContentListAdvancedFilter) => void;
}

/**
 * ContentFilter component provides a filter form to update the search results.
 * There are two types of filters (standard, advanced).
 * The standard filter will execute the 'search' function immediately on a change.
 * The advanced filter will execute the 'search' function only when requested by the user.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentFilter: React.FC<IContentFilterProps> = ({ onSearch }) => {
  const [{ filter, filterAdvanced }, { storeFilter, storeFilterAdvanced }] = useContent();
  const [{ products, ingestTypes, users, sourceOptions, productOptions: pOptions }] =
    useLookupOptions();
  useTooltips();

  const [userOptions, setUserOptions] = React.useState<IOptionItem[]>([]);
  const [productOptions, setProductOptions] = React.useState<IOptionItem[]>([]);

  const timeFrame = timeFrames[Number(filter.timeFrame)];

  React.useEffect(() => {
    // Extract query string values and place them into redux store.
    storeFilter(queryToFilter(filter, window.location.search));
    storeFilterAdvanced(queryToFilterAdvanced(filterAdvanced, window.location.search));
    // Only want this to run on the first load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    setProductOptions([new OptionItem<number>('Any', 0), ...pOptions]);
  }, [pOptions]);

  React.useEffect(() => {
    setUserOptions(
      getUserOptions(
        users.filter((u) => !u.isSystemAccount),
        [new OptionItem<number>('All Users', 0)],
      ),
    );
  }, [users]);

  const onFilterChange = (filter: IContentListFilter) => {
    storeFilter(filter);
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

  const onAdvancedFilterChange = (filter: IContentListAdvancedFilter) => {
    storeFilterAdvanced(filter);
    replaceQueryParams(filter, {
      includeEmpty: false,
      convertObject: (value) => {
        if (instanceOfIOption(value)) return value.value;
        return value.toString();
      },
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
          width={FieldSize.Big}
          defaultValue={productOptions[0]}
          onChange={(newValue) => {
            var productId = !!newValue ? (newValue as IOptionItem).value : 0;
            onFilterChange({
              ...filter,
              pageIndex: 0,
              productId: productId as number,
            });
          }}
        />
        <Select
          name="user"
          label="User"
          width={FieldSize.Big}
          options={userOptions}
          value={userOptions.find((u) => u.value === filter.userId)}
          onChange={(newValue) => {
            var userId = (newValue as IOptionItem).value ?? '';
            onFilterChange({
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
          tooltip="Date published"
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
                checked={filter.contentType === ContentTypeName.PrintContent}
                onChange={(e) => {
                  onFilterChange({
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
                  onFilterChange({
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
                  onFilterChange({
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
                label="Removed"
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
            </div>
          </div>
        </div>
      </div>
      <div className="box">
        <h2 className="caps">Advanced Search</h2>
        <div className="box-content">
          <Select
            name="fieldType"
            label="Field Type"
            options={fieldTypes}
            value={fieldTypes.find((ft) => ft.value === filterAdvanced.fieldType)}
            width={FieldSize.Small}
            onChange={(newValue) => {
              const value =
                newValue instanceof OptionItem ? newValue.toInterface() : (newValue as IOptionItem);
              onAdvancedFilterChange({ ...filterAdvanced, fieldType: value.value, searchTerm: '' });
            }}
          />
          <Show visible={filterAdvanced.fieldType === 'otherSource'}>
            <Select
              name="searchTerm"
              label="Search Terms"
              width={FieldSize.Big}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') onSearch({ ...filter, pageIndex: 0, ...filterAdvanced });
              }}
              onChange={(newValue: any) => {
                const optionItem = sourceOptions.find((ds) => ds.value === newValue.value);
                const newSearchTerm =
                  optionItem?.label.substring(optionItem.label.indexOf('(') + 1).replace(')', '') ??
                  '';
                onAdvancedFilterChange({ ...filterAdvanced, searchTerm: newSearchTerm });
              }}
              options={[new OptionItem('', 0) as IOptionItem].concat([...sourceOptions])}
              value={sourceOptions.find((s) => s.label === filterAdvanced.searchTerm)}
            />
          </Show>
          <Show visible={filterAdvanced.fieldType !== 'otherSource'}>
            <Text
              name="searchTerm"
              label="Search Terms"
              value={filterAdvanced.searchTerm}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') onSearch({ ...filter, pageIndex: 0, ...filterAdvanced });
              }}
              onChange={(e) => {
                onAdvancedFilterChange({ ...filterAdvanced, searchTerm: e.target.value });
              }}
            ></Text>
          </Show>
        </div>
        <Col className="frm-in dateRange" alignItems="flex-start">
          <label data-for="main-tooltip" data-tip="Date created">
            Date Range
          </label>
          <Row>
            <SelectDate
              name="startDate"
              placeholderText="mm/dd/yyyy"
              selected={!!filterAdvanced.startDate ? new Date(filterAdvanced.startDate) : undefined}
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
        </Col>
        <Button
          name="search"
          onClick={() => onSearch({ ...filter, pageIndex: 0, ...filterAdvanced })}
        >
          Search
        </Button>
        <Button
          name="clear"
          variant={ButtonVariant.secondary}
          onClick={() => {
            onAdvancedFilterChange({
              ...initialContentState.filterAdvanced,
            });
            onFilterChange({ ...filter, pageIndex: 0 });
          }}
        >
          Clear
        </Button>
      </div>
    </styled.ContentFilter>
  );
};
