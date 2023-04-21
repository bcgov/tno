import { advancedSearchKeys, advancedSearchOptions } from 'features/content/list-view/constants';
import React from 'react';
import { FaArrowAltCircleRight, FaBinoculars } from 'react-icons/fa';
import { useContent, useLookupOptions } from 'store/hooks';
import {
  ContentStatusName,
  FieldSize,
  filterEnabledOptions,
  getEnumStringOptions,
  IOptionItem,
  OptionItem,
  replaceQueryParams,
  Row,
  Select,
  Show,
  Text,
  ToolBarSection,
} from 'tno-core';

export interface IAdvancedSearchSectionProps {}

/**
 *
 * @param onSearch Inform the parent of the request to search.
 * @param onChange determine what the filter does on change
 * @returns Filter section containing the advanced filter
 */
export const AdvancedSearchSection: React.FC<IAdvancedSearchSectionProps> = () => {
  const [{ sourceOptions }] = useLookupOptions();
  const [
    { filter: oFilter, filterAdvanced: oFilterAdvanced },
    { storeFilter, storeFilterAdvanced },
  ] = useContent();

  const [statusOptions] = React.useState(getEnumStringOptions(ContentStatusName));
  const [filter, setFilter] = React.useState(oFilter);
  const [filterAdvanced, setFilterAdvanced] = React.useState(oFilterAdvanced);

  // keep the filter in sync with the store
  React.useEffect(() => {
    setFilter(oFilter);
  }, [oFilter]);

  const onChange = React.useCallback(() => {
    storeFilter({ ...filter, pageIndex: 0 });
    storeFilterAdvanced(filterAdvanced);
    replaceQueryParams({ ...filter, pageIndex: 0, ...filterAdvanced }, { includeEmpty: false });
  }, [filter, filterAdvanced, storeFilter, storeFilterAdvanced]);

  return (
    <ToolBarSection
      children={
        <Row>
          <Select
            name="fieldType"
            options={advancedSearchOptions}
            className="select"
            width={FieldSize.Medium}
            isClearable={false}
            value={advancedSearchOptions.find((ft) => ft.value === filterAdvanced.fieldType)}
            onChange={(newValue) => {
              const value =
                newValue instanceof OptionItem ? newValue.toInterface() : (newValue as IOptionItem);
              setFilterAdvanced({ ...filterAdvanced, fieldType: value.value, searchTerm: '' });
            }}
          />
          <Show visible={filterAdvanced.fieldType === advancedSearchKeys.Source}>
            <Select
              name="searchTerm"
              width={FieldSize.Medium}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') setFilter({ ...filter, pageIndex: 0 });
              }}
              onChange={(newValue: any) => {
                if (!newValue) setFilterAdvanced({ ...filterAdvanced, searchTerm: '' });
                else {
                  const optionItem = filterEnabledOptions(sourceOptions, newValue.value).find(
                    (ds) => ds.value === newValue.value,
                  );
                  setFilterAdvanced({
                    ...filterAdvanced,
                    searchTerm: optionItem?.value?.toString() ?? '',
                  });
                }
              }}
              options={filterEnabledOptions(sourceOptions, filterAdvanced.searchTerm)}
              value={sourceOptions.find(
                (s) => String(s.value) === String(filterAdvanced.searchTerm),
              )}
            />
          </Show>
          <Show visible={filterAdvanced.fieldType === advancedSearchKeys.Status}>
            <Select
              name="searchTerm"
              width={FieldSize.Medium}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') setFilter({ ...filter, pageIndex: 0 });
              }}
              onChange={(newValue: any) => {
                if (!newValue) setFilterAdvanced({ ...filterAdvanced, searchTerm: '' });
                else {
                  const optionItem = statusOptions.find((ds) => ds.value === newValue.value);
                  setFilterAdvanced({
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
          <Show
            visible={
              ![advancedSearchKeys.Source, advancedSearchKeys.Status].includes(
                filterAdvanced.fieldType,
              )
            }
          >
            <Text
              name="searchTerm"
              width={FieldSize.Small}
              value={filterAdvanced.searchTerm}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') {
                  onChange();
                }
              }}
              onChange={(e) => {
                setFilterAdvanced({ ...filterAdvanced, searchTerm: e.target.value });
              }}
            />
          </Show>
          <FaArrowAltCircleRight onClick={onChange} className="action-button" />
        </Row>
      }
      label="ADVANCED SEARCH"
      icon={<FaBinoculars />}
    />
  );
};
