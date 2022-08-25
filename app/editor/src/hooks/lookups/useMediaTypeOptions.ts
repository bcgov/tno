import { IOptionItem } from 'components/form';
import React from 'react';
import { useLookup } from 'store/hooks';
import { getSortableOptions } from 'utils';

/**
 * Simplify converting media types into options for dropdowns.
 * @returns An array of media types options for dropdowns.
 */
export const useMediaTypeOptions = () => {
  const [{ mediaTypes }] = useLookup();

  const [mediaTypeOptions, setMediaTypeOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    setMediaTypeOptions(getSortableOptions(mediaTypes));
  }, [mediaTypes]);

  return mediaTypeOptions;
};
