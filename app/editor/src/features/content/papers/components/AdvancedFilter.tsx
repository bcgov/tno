import { advancedSearchKeys, advancedSearchOptions } from 'features/content/list-view/constants';
import { IContentListAdvancedFilter } from 'features/content/list-view/interfaces';
import React from 'react';
import { FaArrowAltCircleRight, FaBinoculars } from 'react-icons/fa';
import { useContent } from 'store/hooks';
import {
  Checkbox,
  Col,
  ContentStatusName,
  FieldSize,
  fromQueryString,
  getEnumStringOptions,
  instanceOfIOption,
  IOptionItem,
  OptionItem,
  replaceQueryParams,
  Row,
  Select,
  Show,
  Text,
  ToolBarSection,
} from 'tno-core';

import { IPaperFilter } from '../interfaces';

export interface IAdvancedFilterProps {
  /** The current filter values. */
  filter: IPaperFilter;
  /** Event when filter changes. */
  onFilterChange: (filter: IPaperFilter) => void;
  /** Event fire when user executes search. */
  onSearch: (filter: IPaperFilter & IContentListAdvancedFilter) => void;
}

/**
 * Provides a component to add advanced filters to content.
 * @param props Component properties.
 * @returns Component.
 */
export const AdvancedFilter: React.FC<IAdvancedFilterProps> = ({
  filter,
  onFilterChange,
  onSearch,
}) => {
  const [{ filterPaper, filterPaperAdvanced: filterAdvanced }, { storeFilterPaperAdvanced }] =
    useContent();

  const [statusOptions] = React.useState(getEnumStringOptions(ContentStatusName));

  const search = fromQueryString(window.location.search, {
    arrays: ['contentTypes', 'sourceIds', 'productIds', 'sort'],
    numbers: ['sourceIds', 'productIds'],
  });

  /** initialize advanced search section with query values or new */
  React.useEffect(() => {
    if (!!window.location.search) {
      storeFilterPaperAdvanced({
        ...filterAdvanced,
        fieldType: search.fieldType ?? advancedSearchKeys.Source,
        searchTerm: search.searchTerm ?? '',
      });
    }
    // Only load the URL parameters the first time.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <ToolBarSection label="ADVANCED SEARCH" icon={<FaBinoculars />}>
      <Col gap="0.5em" alignItems="center">
        <Row flex="1 1 0" gap="1em" alignContent="center">
          <Checkbox
            id="chk-top-story"
            label="Top Story"
            checked={filter.topStory}
            tooltip="Content identified as a top story"
            onChange={(e) => {
              onFilterChange({ ...filter, topStory: e.target.checked });
            }}
          />
          <Checkbox
            id="chk-commentary"
            label="Commentary"
            checked={filter.commentary}
            tooltip="Content identified as commentary"
            onChange={(e) => {
              onFilterChange({ ...filter, commentary: e.target.checked });
            }}
          />
          <Checkbox
            id="chk-homepage"
            label="Homepage"
            checked={filter.homepage}
            tooltip="Content identified as a Homepage"
            onChange={(e) => {
              onFilterChange({ ...filter, homepage: e.target.checked });
            }}
          />
        </Row>
        <Row nowrap>
          <Row>
            <Select
              name="fieldType"
              options={advancedSearchOptions.filter((ft) => ft.value !== advancedSearchKeys.Source)}
              className="select"
              isClearable={false}
              value={advancedSearchOptions.find((ft) => ft.value === filterAdvanced.fieldType)}
              onChange={(newValue) => {
                const value =
                  newValue instanceof OptionItem
                    ? newValue.toInterface()
                    : (newValue as IOptionItem);
                storeFilterPaperAdvanced({
                  ...filterAdvanced,
                  fieldType: value.value,
                  searchTerm: '',
                });
              }}
            />
            <Show visible={filterAdvanced.fieldType !== advancedSearchKeys.Status}>
              <Text
                name="searchTerm"
                value={filterAdvanced.searchTerm}
                onKeyUpCapture={(e) => {
                  if (e.key === 'Enter')
                    onSearch({ ...filterPaper, pageIndex: 0, ...filterAdvanced });
                }}
                onChange={(e) => {
                  storeFilterPaperAdvanced({
                    ...filterAdvanced,
                    searchTerm: e.target.value,
                  });
                }}
              />
            </Show>
            <Show visible={filterAdvanced.fieldType === advancedSearchKeys.Status}>
              <Select
                name="searchTerm"
                width={FieldSize.Medium}
                onKeyUpCapture={(e) => {
                  if (e.key === 'Enter')
                    onSearch({ ...filterPaper, pageIndex: 0, ...filterAdvanced });
                }}
                onChange={(newValue: any) => {
                  if (!newValue) storeFilterPaperAdvanced({ ...filterAdvanced, searchTerm: '' });
                  else {
                    const optionItem = statusOptions.find((ds) => ds.value === newValue.value);
                    storeFilterPaperAdvanced({
                      ...filterAdvanced,
                      searchTerm: optionItem?.value?.toString() ?? '',
                    });
                  }
                }}
                options={statusOptions}
                value={statusOptions.find(
                  (s) => String(s.value) === String(filterAdvanced.searchTerm),
                )}
              />
            </Show>
          </Row>
          <Row alignContent="center">
            <FaArrowAltCircleRight
              onClick={() => {
                onSearch({ ...filterPaper, pageIndex: 0, ...filterAdvanced });
                replaceQueryParams(
                  { ...filterPaper, ...filterAdvanced },
                  {
                    includeEmpty: false,
                    convertObject: (value) => {
                      if (instanceOfIOption(value)) return value.value;
                      return value.toString();
                    },
                  },
                );
              }}
              className="action-button"
            />
          </Row>
        </Row>
      </Col>
    </ToolBarSection>
  );
};
