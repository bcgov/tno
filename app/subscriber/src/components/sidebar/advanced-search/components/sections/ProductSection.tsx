import { useLookupOptions } from 'store/hooks';
import { Row, Select } from 'tno-core';

import { IExpandedSectionProps } from '../../interfaces';

/** component that allows user to filter down the advanced search based on products */
export const ProductSection: React.FC<IExpandedSectionProps> = ({
  advancedSearch,
  setAdvancedSearch,
}) => {
  const [{ mediaTypeOptions }] = useLookupOptions();
  return (
    <Row justifyContent="center">
      <Select
        name="productIds"
        isMulti
        width="25em"
        className="products"
        onChange={(newValues) => {
          Array.isArray(newValues) &&
            setAdvancedSearch({
              ...advancedSearch,
              productIds: newValues.map((v) => v.value),
            });
        }}
        options={mediaTypeOptions}
      />
    </Row>
  );
};
