import { IOptionItem } from 'components/form';
import React from 'react';
import { useLookup } from 'store/hooks';
import { getSortableOptions } from 'utils';

/**
 * Simplify converting products into options for dropdowns.
 * @returns An array of products options for dropdowns.
 */
export const useProductOptions = () => {
  const [{ products }] = useLookup();

  const [productOptions, setProductOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    setProductOptions(getSortableOptions(products));
  }, [products]);

  return productOptions;
};
