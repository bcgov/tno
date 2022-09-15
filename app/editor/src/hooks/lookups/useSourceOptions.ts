import { IOptionItem } from 'components/form';
import React from 'react';
import { useLookup } from 'store/hooks';
import { getSourceOptions } from 'utils';

/**
 * Simplify converting data sources into options for dropdowns.
 * @returns An array of data source options for dropdowns.
 */
export const useSourceOptions = () => {
  const [{ sources }] = useLookup();

  const [sourceOptions, setSourceOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    setSourceOptions(getSourceOptions(sources));
  }, [sources]);

  return sourceOptions;
};
