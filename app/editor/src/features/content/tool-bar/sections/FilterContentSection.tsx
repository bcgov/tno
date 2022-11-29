import { IOptionItem, OptionItem, Select } from 'components/form';
import { ToggleGroup } from 'components/toggle-group';
import { IContentListFilter } from 'features/content/list-view/interfaces';
import { useLookupOptions } from 'hooks';
import React from 'react';
import { FaCalendarAlt, FaClock, FaFilter, FaIcons, FaUsers } from 'react-icons/fa';
import { useApp, useContent } from 'store/hooks';
import { Col, FieldSize, Row } from 'tno-core';

import * as styled from './styled';

export interface IFilterContentSectionProps {
  onChange: (filter: IContentListFilter) => void;
}

/**
 * Component containing the filter section of the content tool bar
 * @param onChange determine what happens when filter changes are applied
 * @returns The filter content section
 */
export const FilterContentSection: React.FC<IFilterContentSectionProps> = ({ onChange }) => {
  const [{ filter }] = useContent();
  const [{ productOptions: pOptions }] = useLookupOptions();
  const [productOptions, setProductOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    setProductOptions([new OptionItem<number>('Any', 0), ...pOptions]);
  }, [pOptions]);

  const [{ userInfo }] = useApp();

  return (
    <styled.FilterContentSection
      actions={
        <Row>
          <Col>
            <Row>
              <FaClock className="icon-indicator" />
              <ToggleGroup
                options={[
                  {
                    label: 'Today',
                    onClick: () => {
                      onChange({ ...filter, timeFrame: 0 });
                    },
                  },
                  { label: '24 HRS', onClick: () => onChange({ ...filter, timeFrame: 1 }) },
                  { label: '48 HRS', onClick: () => onChange({ ...filter, timeFrame: 2 }) },
                  { label: 'ALL', onClick: () => onChange({ ...filter, timeFrame: 3 }) },
                ]}
              />
              <FaCalendarAlt className="action-icon" />
            </Row>
            <Row>
              <FaUsers className="icon-indicator" />
              <ToggleGroup
                options={[
                  {
                    label: 'ALL CONTENT',
                    onClick: () => onChange({ ...filter, userId: 0 }),
                  },
                  {
                    label: 'MY CONTENT',
                    onClick: () => onChange({ ...filter, userId: userInfo?.id ?? 0 }),
                  },
                  { label: 'OTHER' },
                ]}
              />
            </Row>
          </Col>
          <Col>
            <Row>
              <FaIcons className="icon-indicator" height="2em" width="2em" />
              <Select
                className="select"
                name="productId"
                placeholder="Designation"
                options={productOptions}
                value={productOptions.find((mt) => mt.value === filter.productId)}
                width={FieldSize.Big}
                defaultValue={productOptions[0]}
                onChange={(newValue) => {
                  var productId = !!newValue ? (newValue as IOptionItem).value : 0;
                  onChange({
                    ...filter,
                    pageIndex: 0,
                    productId: productId as number,
                  });
                }}
              />
            </Row>
          </Col>
        </Row>
      }
      label="FILTER CONTENT"
      icon={<FaFilter />}
    />
  );
};
