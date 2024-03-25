import { AdvancedSearchKeys, advancedSearchOptions } from 'features/content/constants';
import { IContentListAdvancedFilter, IContentListFilter } from 'features/content/interfaces';
import moment from 'moment';
import React from 'react';
import { FaArrowAltCircleRight, FaBinoculars } from 'react-icons/fa';
import { useContent, useLookupOptions } from 'store/hooks';
import {
  Checkbox,
  Col,
  ContentStatusName,
  FieldSize,
  filterEnabledOptions,
  fromQueryString,
  getEnumStringOptions,
  instanceOfIOption,
  IOptionItem,
  OptionItem,
  replaceQueryParams,
  Row,
  Select,
  SelectDate,
  Show,
  Text,
  ToolBarSection,
} from 'tno-core';

export interface IAdvancedFilterProps {
  /** The current filter values. */
  filter: IContentListFilter;
  /** Event when filter changes. */
  onFilterChange: (filter: IContentListFilter) => void;
  /** Event fire when user executes search. */
  onSearch: (filter: IContentListFilter & IContentListAdvancedFilter) => void;
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
  const [{ sourceOptions }] = useLookupOptions();

  const [statusOptions] = React.useState(getEnumStringOptions(ContentStatusName));

  const search = fromQueryString(window.location.search, {
    arrays: ['contentTypes', 'sourceIds', 'mediaTypeIds', 'sort'],
    numbers: ['sourceIds', 'mediaTypeIds'],
  });

  /** initialize advanced search section with query values or new */
  React.useEffect(() => {
    if (!!window.location.search) {
      storeFilterPaperAdvanced({
        ...filterAdvanced,
        fieldType: search.fieldType ?? AdvancedSearchKeys.Source,
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
            id="chk-featuredStories"
            label="Featured Stories"
            checked={filter.featuredStory}
            tooltip="Content identified as a feature stories"
            onChange={(e) => {
              onFilterChange({ ...filter, featuredStory: e.target.checked });
            }}
          />
          <Checkbox
            id="chk-published"
            label="Published"
            checked={filter.onlyPublished}
            tooltip="Published content"
            onChange={(e) => {
              onFilterChange({ ...filter, onlyPublished: e.target.checked });
            }}
          />
        </Row>
        <Row nowrap>
          <Row>
            <Select
              name="fieldType"
              options={advancedSearchOptions}
              className="select"
              width="20ch"
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
            <Show
              visible={
                ![
                  AdvancedSearchKeys.Source,
                  AdvancedSearchKeys.Status,
                  AdvancedSearchKeys.PublishedOn,
                  AdvancedSearchKeys.CreatedOn,
                  AdvancedSearchKeys.UpdatedOn,
                ].includes(filterAdvanced.fieldType)
              }
            >
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
            <Show visible={filterAdvanced.fieldType === AdvancedSearchKeys.Status}>
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
            <Show visible={filterAdvanced.fieldType === AdvancedSearchKeys.Source}>
              <Select
                name="searchTerm"
                width={FieldSize.Medium}
                onChange={(newValue: any) => {
                  if (!newValue) storeFilterPaperAdvanced({ ...filterAdvanced, searchTerm: '' });
                  else {
                    const optionItem = filterEnabledOptions(sourceOptions, newValue.value).find(
                      (ds) => ds.value === newValue.value,
                    );
                    storeFilterPaperAdvanced({
                      ...filterAdvanced,
                      searchTerm: optionItem?.value?.toString() ?? '',
                    });
                  }
                }}
                options={filterEnabledOptions(sourceOptions, filterAdvanced.searchTerm)}
                value={sourceOptions.find(
                  (s: IOptionItem) => String(s.value) === String(filterAdvanced.searchTerm),
                )}
              />
            </Show>
            <Show
              visible={[
                AdvancedSearchKeys.Status,
                AdvancedSearchKeys.PublishedOn,
                AdvancedSearchKeys.CreatedOn,
                AdvancedSearchKeys.UpdatedOn,
              ].includes(filterAdvanced.fieldType)}
            >
              <SelectDate
                name="searchTeam"
                width="15ch"
                isClearable
                value={
                  moment(filterAdvanced.searchTerm).isValid()
                    ? filterAdvanced.searchTerm
                    : undefined
                }
                onChange={(value) => {
                  storeFilterPaperAdvanced({
                    ...filterAdvanced,
                    searchTerm: moment(value).format('YYYY-MM-DD'),
                  });
                }}
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
