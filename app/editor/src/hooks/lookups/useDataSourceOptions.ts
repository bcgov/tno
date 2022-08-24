import { IOptionItem } from 'components/form';
import React from 'react';
import { useLookup } from 'store/hooks';
import { getDataSourceOptions } from 'utils';

/**
 * Simplify converting data sources into options for dropdowns.
 * @returns An array of data source options for dropdowns.
 */
export const useDataSourceOptions = () => {
  const [{ dataSources }] = useLookup();

  const [dataSourceOptions, setDataSourceOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    setDataSourceOptions(getDataSourceOptions(dataSources));
  }, [dataSources]);

  return dataSourceOptions;
};
