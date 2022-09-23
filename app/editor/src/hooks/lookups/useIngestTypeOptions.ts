import { IOptionItem } from 'components/form';
import React from 'react';
import { useLookup } from 'store/hooks';
import { getSortableOptions } from 'utils';

/**
 * Simplify converting ingest types into options for dropdowns.
 * @returns An array of ingest types options for dropdowns.
 */
export const useIngestTypeOptions = () => {
  const [{ ingestTypes }] = useLookup();

  const [ingestTypeOptions, setIngestTypeOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    setIngestTypeOptions(getSortableOptions(ingestTypes));
  }, [ingestTypes]);

  return ingestTypeOptions;
};
