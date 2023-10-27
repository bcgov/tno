import { AdvancedSearchKeys, advancedSearchOptions } from 'features/content/constants';
import React from 'react';
import { FaArrowAltCircleRight, FaBinoculars } from 'react-icons/fa';
import { FaX } from 'react-icons/fa6';
import { useContent, useLookupOptions } from 'store/hooks';
import {
  Button,
  ButtonVariant,
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
  const [{ filter: oFilter, filterAdvanced }, { storeFilter, storeFilterAdvanced }] = useContent();

  const [statusOptions] = React.useState(getEnumStringOptions(ContentStatusName));
  const [filter, setFilter] = React.useState(oFilter);

  // keep the filter in sync with the store
  React.useEffect(() => {
    setFilter(oFilter);
  }, [oFilter]);

  const onChange = React.useCallback(() => {
    storeFilter({ ...filter, pageIndex: 0 });
    storeFilterAdvanced({
      ...filterAdvanced,
      startDate: filterAdvanced.startDate,
      endDate: filterAdvanced.endDate,
    });
    replaceQueryParams({ ...filter, pageIndex: 0, ...filterAdvanced }, { includeEmpty: false });
    // eslint-disable-next-line react-hooks/exhaustive-deps
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
              storeFilterAdvanced({ ...filterAdvanced, fieldType: value.value, searchTerm: '' });
            }}
          />
          <Show visible={filterAdvanced.fieldType === AdvancedSearchKeys.Source}>
            <Select
              name="searchTerm"
              width={FieldSize.Medium}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') setFilter({ ...filter, pageIndex: 0 });
              }}
              onChange={(newValue: any) => {
                if (!newValue) storeFilterAdvanced({ ...filterAdvanced, searchTerm: '' });
                else {
                  const optionItem = filterEnabledOptions(sourceOptions, newValue.value).find(
                    (ds) => ds.value === newValue.value,
                  );
                  storeFilterAdvanced({
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
          <Show visible={filterAdvanced.fieldType === AdvancedSearchKeys.Status}>
            <Select
              name="searchTerm"
              width={FieldSize.Medium}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') setFilter({ ...filter, pageIndex: 0 });
              }}
              onChange={(newValue: any) => {
                if (!newValue) storeFilterAdvanced({ ...filterAdvanced, searchTerm: '' });
                else {
                  const optionItem = statusOptions.find((ds) => ds.value === newValue.value);
                  storeFilterAdvanced({
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
              ![AdvancedSearchKeys.Source, AdvancedSearchKeys.Status].includes(
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
                storeFilterAdvanced({ ...filterAdvanced, searchTerm: e.target.value });
              }}
            >
              <Button
                variant={ButtonVariant.secondary}
                onClick={() => {
                  storeFilterAdvanced({ ...filterAdvanced, searchTerm: '' });
                }}
              >
                <FaX />
              </Button>
            </Text>
          </Show>
          <FaArrowAltCircleRight onClick={onChange} className="action-button" />
        </Row>
      }
      label="ADVANCED SEARCH"
      icon={<FaBinoculars />}
    />
  );
};
