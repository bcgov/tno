import { AdvancedSearchKeys, advancedSearchOptions } from 'features/content/constants';
import { IContentListAdvancedFilter } from 'features/content/interfaces';
import moment from 'moment';
import React from 'react';
import { FaArrowAltCircleRight, FaBinoculars } from 'react-icons/fa';
import { FaX } from 'react-icons/fa6';
import { useContent, useLookupOptions } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  ContentStatusName,
  FieldSize,
  filterEnabledOptions,
  getEnumStringOptions,
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

export interface IAdvancedSearchSectionProps {}

/**
 *
 * @param onSearch Inform the parent of the request to search.
 * @param onChange determine what the filter does on change
 * @returns Filter section containing the advanced filter
 */
export const AdvancedSearchSection: React.FC<IAdvancedSearchSectionProps> = () => {
  const [{ sourceOptions, seriesOptions, series }] = useLookupOptions();
  const [{ filter: oFilter, filterAdvanced }, { storeFilter, storeFilterAdvanced }] = useContent();

  const [statusOptions] = React.useState(getEnumStringOptions(ContentStatusName));
  const [filter, setFilter] = React.useState(oFilter);

  const showsAndProgramsOptions = React.useMemo(() => {
    const allowedSeries = new Set(series.filter((s) => s.isEnabled && !s.isOther).map((s) => s.id));
    return seriesOptions.filter(
      (option) => option.value !== undefined && allowedSeries.has(Number(option.value)),
    );
  }, [series, seriesOptions]);

  const updateAdvancedFilter = React.useCallback(
    (values: Partial<IContentListAdvancedFilter>) => {
      storeFilterAdvanced({ ...filterAdvanced, ...values });
    },
    [filterAdvanced, storeFilterAdvanced],
  );

  const onChange = React.useCallback(() => {
    storeFilter({ ...filter, pageIndex: 0 });
    storeFilterAdvanced({ ...filterAdvanced });
    const queryParams: Record<string, unknown> = {
      ...filter,
      pageIndex: 0,
      fieldType: filterAdvanced.fieldType,
      searchTerm: filterAdvanced.searchTerm,
    };
    if (filterAdvanced.secondarySearchTerm) {
      queryParams.secondaryFieldType = filterAdvanced.secondaryFieldType;
      queryParams.secondarySearchTerm = filterAdvanced.secondarySearchTerm;
    }
    replaceQueryParams(queryParams, { includeEmpty: false });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, filterAdvanced, storeFilter, storeFilterAdvanced]);

  const buildAdvancedRow = React.useCallback(
    (rowKey: 'primary' | 'secondary') => {
      const isSecondary = rowKey === 'secondary';
      const fieldTypeKey = isSecondary ? 'secondaryFieldType' : 'fieldType';
      const searchTermKey = isSecondary ? 'secondarySearchTerm' : 'searchTerm';

      const currentFieldType = (filterAdvanced as any)[fieldTypeKey] ?? AdvancedSearchKeys.Source;
      const currentSearchTerm = (filterAdvanced as any)[searchTermKey] ?? '';

      const setFieldType = (value: AdvancedSearchKeys) =>
        updateAdvancedFilter({
          [fieldTypeKey]: value,
          [searchTermKey]: '',
        } as Partial<IContentListAdvancedFilter>);

      const setSearchTerm = (value: string) =>
        updateAdvancedFilter({
          [searchTermKey]: value,
        } as Partial<IContentListAdvancedFilter>);

      return (
        <Row
          key={rowKey}
          data-testid={`advanced-search-row-${rowKey}`}
          gap="0.25rem"
          alignItems="center"
        >
          <Select
            name={isSecondary ? 'fieldTypeSecondary' : 'fieldType'}
            options={advancedSearchOptions}
            className="select"
            width={FieldSize.Medium}
            isClearable={false}
            value={advancedSearchOptions.find((ft) => ft.value === currentFieldType)}
            onChange={(newValue) => {
              const option =
                newValue instanceof OptionItem ? newValue.toInterface() : (newValue as IOptionItem);
              setFieldType(option.value as AdvancedSearchKeys);
            }}
          />
          <Show visible={currentFieldType === AdvancedSearchKeys.Source}>
            <Select
              name={isSecondary ? 'searchTermSecondarySource' : 'searchTermSource'}
              width={FieldSize.Medium}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') setFilter({ ...filter, pageIndex: 0 });
              }}
              onChange={(newValue: any) => {
                if (!newValue) setSearchTerm('');
                else {
                  const optionItem = filterEnabledOptions(sourceOptions, newValue.value).find(
                    (ds) => ds.value === newValue.value,
                  );
                  setSearchTerm(optionItem?.value?.toString() ?? '');
                }
              }}
              options={filterEnabledOptions(sourceOptions, currentSearchTerm)}
              value={sourceOptions.find((s) => String(s.value) === String(currentSearchTerm))}
            />
          </Show>
          <Show visible={currentFieldType === AdvancedSearchKeys.Status}>
            <Select
              name={isSecondary ? 'searchTermSecondaryStatus' : 'searchTermStatus'}
              width={FieldSize.Medium}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') setFilter({ ...filter, pageIndex: 0 });
              }}
              onChange={(newValue: any) => {
                if (!newValue) setSearchTerm('');
                else {
                  const optionItem = statusOptions.find((ds) => ds.value === newValue.value);
                  setSearchTerm(optionItem?.value?.toString() ?? '');
                }
              }}
              options={statusOptions}
              value={statusOptions.find((s) => String(s.value) === String(currentSearchTerm))}
            />
          </Show>
          <Show visible={currentFieldType === AdvancedSearchKeys.Series}>
            <Select
              name={isSecondary ? 'searchTermSecondarySeries' : 'searchTermSeries'}
              width={FieldSize.Medium}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') setFilter({ ...filter, pageIndex: 0 });
              }}
              onChange={(newValue: any) => {
                if (!newValue) setSearchTerm('');
                else setSearchTerm(newValue.value?.toString() ?? '');
              }}
              options={showsAndProgramsOptions}
              value={showsAndProgramsOptions.find(
                (o) => String(o.value) === String(currentSearchTerm),
              )}
            />
          </Show>
          <Show
            visible={[
              AdvancedSearchKeys.Status,
              AdvancedSearchKeys.PublishedOn,
              AdvancedSearchKeys.CreatedOn,
              AdvancedSearchKeys.UpdatedOn,
            ].includes(currentFieldType)}
          >
            <SelectDate
              name={isSecondary ? 'searchTermSecondaryDate' : 'searchTermDate'}
              width="15ch"
              isClearable
              value={moment(currentSearchTerm).isValid() ? currentSearchTerm : undefined}
              onChange={(value) => {
                setSearchTerm(value ? moment(value).format('YYYY-MM-DD') : '');
              }}
            />
          </Show>
          <Show
            visible={
              ![
                AdvancedSearchKeys.Source,
                AdvancedSearchKeys.Series,
                AdvancedSearchKeys.Status,
                AdvancedSearchKeys.PublishedOn,
                AdvancedSearchKeys.CreatedOn,
                AdvancedSearchKeys.UpdatedOn,
              ].includes(currentFieldType)
            }
          >
            <Text
              name={isSecondary ? 'searchTermSecondary' : 'searchTerm'}
              width={FieldSize.Small}
              value={currentSearchTerm}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') {
                  setFilter({ ...filter, pageIndex: 0 });
                  onChange();
                }
              }}
              onChange={(e) => {
                setSearchTerm(e.target.value);
              }}
            >
              <Button
                variant={ButtonVariant.secondary}
                onClick={() => {
                  setSearchTerm('');
                }}
              >
                <FaX />
              </Button>
            </Text>
          </Show>
        </Row>
      );
    },
    [
      filter,
      filterAdvanced,
      onChange,
      setFilter,
      showsAndProgramsOptions,
      sourceOptions,
      statusOptions,
      updateAdvancedFilter,
    ],
  );

  // keep the filter in sync with the store
  React.useEffect(() => {
    setFilter(oFilter);
  }, [oFilter]);

  return (
    <ToolBarSection
      children={
        <Row gap="0.5rem" alignItems="center">
          <Col gap="0.5rem">
            {buildAdvancedRow('primary')}
            {buildAdvancedRow('secondary')}
          </Col>
          <FaArrowAltCircleRight onClick={onChange} className="action-button" />
        </Row>
      }
      label="ADVANCED SEARCH"
      icon={<FaBinoculars />}
    />
  );
};
