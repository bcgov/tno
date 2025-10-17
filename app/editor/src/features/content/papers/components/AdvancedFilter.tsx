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
  const [{ sourceOptions, seriesOptions, series }] = useLookupOptions();

  const [statusOptions] = React.useState(getEnumStringOptions(ContentStatusName));

  const showsAndProgramsOptions = React.useMemo(() => {
    const allowedSeries = new Set(series.filter((s) => s.isEnabled && !s.isOther).map((s) => s.id));
    return seriesOptions.filter(
      (option) => option.value !== undefined && allowedSeries.has(Number(option.value)),
    );
  }, [series, seriesOptions]);

  const updateAdvancedFilter = React.useCallback(
    (values: Partial<IContentListAdvancedFilter>) => {
      storeFilterPaperAdvanced({ ...filterAdvanced, ...values });
    },
    [filterAdvanced, storeFilterPaperAdvanced],
  );

  const executeSearch = React.useCallback(() => {
    onSearch({ ...filterPaper, pageIndex: 0, ...filterAdvanced });
    const queryParams: Record<string, unknown> = {
      ...filterPaper,
      pageIndex: 0,
      fieldType: filterAdvanced.fieldType,
      searchTerm: filterAdvanced.searchTerm,
    };
    if (filterAdvanced.secondarySearchTerm) {
      queryParams.secondaryFieldType = filterAdvanced.secondaryFieldType;
      queryParams.secondarySearchTerm = filterAdvanced.secondarySearchTerm;
    }
    replaceQueryParams(queryParams, {
      includeEmpty: false,
      convertObject: (value) => {
        if (instanceOfIOption(value)) return value.value;
        return value.toString();
      },
    });
  }, [filterAdvanced, filterPaper, onSearch]);

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
          data-testid={`paper-advanced-row-${rowKey}`}
          gap="0.5em"
          alignItems="center"
        >
          <Select
            name={isSecondary ? 'paperFieldTypeSecondary' : 'paperFieldType'}
            options={advancedSearchOptions}
            className="select"
            width="20ch"
            isClearable={false}
            value={advancedSearchOptions.find((ft) => ft.value === currentFieldType)}
            onChange={(newValue) => {
              const option =
                newValue instanceof OptionItem ? newValue.toInterface() : (newValue as IOptionItem);
              setFieldType(option.value as AdvancedSearchKeys);
            }}
          />
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
              name={isSecondary ? 'paperSearchTermSecondary' : 'paperSearchTerm'}
              value={currentSearchTerm}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') executeSearch();
              }}
              onChange={(e) => {
                setSearchTerm(e.target.value);
              }}
            />
          </Show>
          <Show visible={currentFieldType === AdvancedSearchKeys.Status}>
            <Select
              name={isSecondary ? 'paperSearchTermSecondaryStatus' : 'paperSearchTermStatus'}
              width={FieldSize.Medium}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') executeSearch();
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
          <Show visible={currentFieldType === AdvancedSearchKeys.Source}>
            <Select
              name={isSecondary ? 'paperSearchTermSecondarySource' : 'paperSearchTermSource'}
              width={FieldSize.Medium}
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
              value={sourceOptions.find(
                (s: IOptionItem) => String(s.value) === String(currentSearchTerm),
              )}
            />
          </Show>
          <Show visible={currentFieldType === AdvancedSearchKeys.Series}>
            <Select
              name={isSecondary ? 'paperSearchTermSecondarySeries' : 'paperSearchTermSeries'}
              width={FieldSize.Medium}
              onChange={(newValue: any) => {
                if (!newValue) setSearchTerm('');
                else setSearchTerm(newValue.value?.toString() ?? '');
              }}
              options={showsAndProgramsOptions}
              value={showsAndProgramsOptions.find(
                (s: IOptionItem) => String(s.value) === String(currentSearchTerm),
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
              name={isSecondary ? 'paperSearchTermSecondaryDate' : 'paperSearchTermDate'}
              width="15ch"
              isClearable
              value={moment(currentSearchTerm).isValid() ? currentSearchTerm : undefined}
              onChange={(value) => {
                setSearchTerm(value ? moment(value).format('YYYY-MM-DD') : '');
              }}
            />
          </Show>
        </Row>
      );
    },
    [
      executeSearch,
      filterAdvanced,
      showsAndProgramsOptions,
      sourceOptions,
      statusOptions,
      updateAdvancedFilter,
    ],
  );

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
        secondaryFieldType: search.secondaryFieldType ?? filterAdvanced.secondaryFieldType,
        secondarySearchTerm: search.secondarySearchTerm ?? filterAdvanced.secondarySearchTerm,
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
          <Col gap="0.5em">
            {buildAdvancedRow('primary')}
            {buildAdvancedRow('secondary')}
          </Col>
          <Row alignContent="center">
            <FaArrowAltCircleRight onClick={executeSearch} className="action-button" />
          </Row>
        </Row>
      </Col>
    </ToolBarSection>
  );
};
