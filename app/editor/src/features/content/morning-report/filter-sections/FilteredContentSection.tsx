import { InputOption } from 'features/content/tool-bar/sections/filter';
import React from 'react';
import { FaEye, FaFilter, FaIcons, FaNewspaper } from 'react-icons/fa';
import { useLookup, useLookupOptions } from 'store/hooks';
import {
  Col,
  FieldSize,
  filterEnabledOptions,
  IOptionItem,
  OptionItem,
  Row,
  Select,
  ToggleGroup,
  ToolBarSection,
} from 'tno-core';

import { defaultReportFilter } from '../constants/defaultReportFilter';
import { IMorningReportFilter } from '../interfaces';

export interface IFilteredContentSection {
  onFilterChange: (filter: IMorningReportFilter) => void;
  filter: IMorningReportFilter;
  onSearch: (filter: IMorningReportFilter) => void;
}

/** The filter content section of the morning report */
export const FilteredContentSection: React.FC<IFilteredContentSection> = ({
  onFilterChange,
  filter,
  onSearch,
}) => {
  const [{ productOptions: pOptions, sourceOptions }] = useLookupOptions();
  const [productOptions, setProductOptions] = React.useState<IOptionItem[]>([]);
  const [{ sources }] = useLookup();

  /** TODO: Filter needs to apply the query string to the filter when reloading - same issue exists on content list view **/
  /** Extracts variables that have multiple instances from the query */
  const getQueryVariable = (variable: string) => {
    const multiSelect = [];
    var query = window.location.search.substring(1);
    var vars = query.split('&');
    for (var i = 0; i < vars.length; i++) {
      var pair = vars[i].split('=');
      if (decodeURIComponent(pair[0]) === variable) {
        multiSelect.push(decodeURIComponent(pair[1]));
      }
    }
    return multiSelect;
  };

  React.useEffect(() => {
    setProductOptions([new OptionItem<number>('Any', 0), ...pOptions]);
  }, [pOptions]);

  return (
    <ToolBarSection
      children={
        <Col>
          <Row>
            <FaEye className="icon-indicator" />
            <ToggleGroup
              defaultSelected="ALL TODAYS CONTENT"
              options={[
                {
                  label: 'ALL TODAYS CONTENT',
                  onClick: () => onFilterChange(defaultReportFilter(sources)),
                },
                {
                  label: 'PREVIEW A.M. REPORT',
                },
                {
                  label: 'SHOW HIDDEN ONLY',
                  onClick: (e) => {
                    onFilterChange({
                      ...filter,
                      pageIndex: 0,
                      includeHidden: true,
                    });
                  },
                },
              ]}
            />
            <FaIcons className="icon-indicator" height="2em" width="2em" />
            <Select
              className="select"
              name="productIds"
              placeholder="Designation"
              isMulti
              closeMenuOnSelect={false}
              hideSelectedOptions={false}
              options={filterEnabledOptions(productOptions)}
              width={FieldSize.Big}
              defaultValue={productOptions[0]}
              components={{
                Option: InputOption,
              }}
              onChange={(newValues) => {
                const productIds = Array.isArray(newValues)
                  ? newValues.map((opt) => opt.value)
                  : [0];
                onFilterChange({
                  ...filter,
                  pageIndex: 0,
                  productIds: productIds,
                });
              }}
            />
          </Row>

          <Row className="filter-select sources">
            <FaNewspaper className="icon-indicator" />
            <Select
              name="sourceIds"
              isMulti
              width={'48.25em'}
              placeholder="Sources"
              options={sourceOptions}
              value={sourceOptions.filter((opt) =>
                getQueryVariable('sourceIds').includes(opt.value?.toString() ?? ''),
              )}
              onChange={(newValues) => {
                const sourceIds = Array.isArray(newValues)
                  ? newValues.map((opt) => opt.value)
                  : [0];
                onFilterChange({
                  ...filter,
                  pageIndex: 0,
                  sourceIds: sourceIds,
                });
              }}
            />
          </Row>
        </Col>
      }
      label="FILTER CONTENT"
      icon={<FaFilter />}
    />
  );
};
