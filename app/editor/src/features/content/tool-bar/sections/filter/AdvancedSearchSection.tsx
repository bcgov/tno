import { ToolBarSection } from 'components/tool-bar';
import { fieldTypes } from 'features/content/list-view/constants';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { useLookupOptions } from 'hooks';
import { FaArrowAltCircleRight, FaBinoculars } from 'react-icons/fa';
import { useContent } from 'store/hooks';
import { FieldSize, IOptionItem, OptionItem, Row, Select, Show, Text } from 'tno-core';

export interface IAdvancedSearchSectionProps {
  onChange: (filter: IContentListAdvancedFilter) => void;
  onSearch: (filter: IContentListFilter & IContentListAdvancedFilter) => void;
}

/**
 *
 * @param onSearch Inform the parent of the request to search.
 * @param onChange determine what the filter does on change
 * @returns Filter section containing the advanced filter
 */
export const AdvancedSearchSection: React.FC<IAdvancedSearchSectionProps> = ({
  onChange,
  onSearch,
}) => {
  const [{ sourceOptions }] = useLookupOptions();
  const [{ filterAdvanced, filter }] = useContent();
  return (
    <ToolBarSection
      children={
        <Row>
          <Select
            name="fieldType"
            options={fieldTypes}
            className="select"
            width={FieldSize.Medium}
            value={fieldTypes.find((ft) => ft.value === filterAdvanced.fieldType)}
            onChange={(newValue) => {
              const value =
                newValue instanceof OptionItem ? newValue.toInterface() : (newValue as IOptionItem);
              onChange({ ...filterAdvanced, fieldType: value.value, searchTerm: '' });
            }}
          />
          <Show visible={filterAdvanced.fieldType === 'otherSource'}>
            <Select
              name="searchTerm"
              width={FieldSize.Medium}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') onSearch({ ...filter, pageIndex: 0, ...filterAdvanced });
              }}
              onChange={(newValue: any) => {
                if (!newValue) onChange({ ...filterAdvanced, searchTerm: '' });
                else {
                  const optionItem = sourceOptions.find((ds) => ds.value === newValue.value);
                  const newSearchTerm =
                    optionItem?.label
                      .substring(optionItem.label.indexOf('(') + 1)
                      .replace(')', '') ?? '';
                  onChange({ ...filterAdvanced, searchTerm: newSearchTerm });
                }
              }}
              options={[new OptionItem('', 0) as IOptionItem].concat([...sourceOptions])}
              value={sourceOptions.find((s) => s.label === filterAdvanced.searchTerm)}
            />
          </Show>
          <Show visible={filterAdvanced.fieldType !== 'otherSource'}>
            <Text
              name="searchTerm"
              width={FieldSize.Small}
              value={filterAdvanced.searchTerm}
              onKeyUpCapture={(e) => {
                if (e.key === 'Enter') onSearch({ ...filter, pageIndex: 0, ...filterAdvanced });
              }}
              onChange={(e) => {
                onChange({ ...filterAdvanced, searchTerm: e.target.value });
              }}
            />
          </Show>
          <FaArrowAltCircleRight
            onClick={() => onSearch({ ...filter, pageIndex: 0, ...filterAdvanced })}
            className="action-button"
          />
        </Row>
      }
      label="ADVANCED SEARCH"
      icon={<FaBinoculars />}
    />
  );
};
