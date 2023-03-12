import { ToolBarSection } from 'components/tool-bar';
import { fieldTypes } from 'features/content/list-view/constants';
import { IContentListAdvancedFilter } from 'features/content/list-view/interfaces';
import React from 'react';
import { FaArrowAltCircleRight, FaBinoculars } from 'react-icons/fa';
import { useContent } from 'store/hooks';
import {
  FieldSize,
  fromQueryString,
  instanceOfIOption,
  IOptionItem,
  OptionItem,
  replaceQueryParams,
  Row,
  Select,
  Text,
} from 'tno-core';

import { IMorningReportFilter } from './interfaces';

export interface IAdvancedSearchSectionProps {
  onSearch: (filter: IMorningReportFilter & IContentListAdvancedFilter) => void;
}

/**
 *
 * @param onSearch Inform the parent of the request to search.
 * @returns Filter section containing the advanced filter
 */
export const AdvancedMorningReport: React.FC<IAdvancedSearchSectionProps> = ({ onSearch }) => {
  const [{ morningReportFilter, filterAdvanced, filter }, { storeFilterAdvanced }] = useContent();
  const search = fromQueryString(window.location.search);
  /** initialize advanced search section with query values or new */
  React.useEffect(() => {
    storeFilterAdvanced({
      ...filterAdvanced,
      fieldType: search.fieldType ?? fieldTypes[0].value,
      searchTerm: search.searchTerm ?? '',
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
  return (
    <ToolBarSection
      children={
        <Row>
          <Select
            name="fieldType"
            options={fieldTypes.filter((ft) => ft.label !== 'Source')}
            className="select"
            width={FieldSize.Medium}
            isClearable={false}
            value={fieldTypes.find((ft) => ft.value === filterAdvanced.fieldType)}
            onChange={(newValue) => {
              const value =
                newValue instanceof OptionItem ? newValue.toInterface() : (newValue as IOptionItem);
              storeFilterAdvanced({ ...filterAdvanced, fieldType: value.value, searchTerm: '' });
            }}
          />
          <Text
            name="searchTerm"
            width={FieldSize.Small}
            value={filterAdvanced.searchTerm}
            onKeyUpCapture={(e) => {
              if (e.key === 'Enter')
                onSearch({ ...morningReportFilter, pageIndex: 0, ...filterAdvanced });
            }}
            onChange={(e) => {
              storeFilterAdvanced({ ...filterAdvanced, searchTerm: e.target.value });
            }}
          />
          <FaArrowAltCircleRight
            onClick={() => {
              onSearch({ ...morningReportFilter, pageIndex: 0, ...filterAdvanced });
              replaceQueryParams(
                { ...filter, ...filterAdvanced },
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
      }
      label="ADVANCED SEARCH"
      icon={<FaBinoculars />}
    />
  );
};
